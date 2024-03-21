#include <math.h>
#include "trajectoryGenerator.h"
#include "timer.h"
#include "Robot.h"
#include "utilities.h"

volatile GhostPosition ghostPosition;
static unsigned long lastUpdateTime = 0;

void InitTrajectoryGenerator(void) {
    ghostPosition.x = 0.0;
    ghostPosition.y = 0.0;
    ghostPosition.theta = 0.0;
    ghostPosition.linearSpeed = 0.0;
    ghostPosition.angularSpeed = 0.0;
    ghostPosition.targetX = 0.0;
    ghostPosition.targetY = 0.0;
    ghostPosition.angleToTarget = 0.0;
    ghostPosition.distanceToTarget = 0.0;
    ghostPosition.state = IDLE;
}

void UpdateTrajectory() // Mise a jour de la trajectoire en fonction de l'etat actuel
{
    switch (ghostPosition.state) {
        case IDLE:
            ghostPosition.linearSpeed = 0.0;
            ghostPosition.angularSpeed = 0.0;

            if (ghostPosition.targetX != 0.0 || ghostPosition.targetY != 0.0) // Verifier si une nouvelle cible est definie
            {
                // Determiner l'angle actuel du robot par rapport a la cible
                double angleToTarget = atan2(ghostPosition.targetY - ghostPosition.y, ghostPosition.targetX - ghostPosition.x);
                double angleDifference = ModuloByAngle(ghostPosition.theta, angleToTarget - ghostPosition.theta);

                // Si le robot n'est pas axe vers la cible, commencer par tourner
                if (fabs(angleDifference) > ANGLE_TOLERANCE) {
                    ghostPosition.state = ROTATING;
                } else {
                    // Si le robot est deja oriente vers la cible, passer directement a l'avancement
                    ghostPosition.state = ADVANCING;
                }
            }
            break;

        case ROTATING:
            RotateTowardsTarget(timestamp);
            break;

        case ADVANCING:
            AdvanceTowardsTarget(timestamp);
            break;
    }
    lastUpdateTime = timestamp;
    robotState.consigneLin = ghostPosition.linearSpeed;
    robotState.consigneAng = ghostPosition.angularSpeed;
}

void RotateTowardsTarget(double currentTime) // Orientation du Ghost vers le waypoint
{
    double deltaTime = (currentTime - lastUpdateTime) / 1000.0;
    double thetaWaypoint = atan2(ghostPosition.targetY - ghostPosition.y, ghostPosition.targetX - ghostPosition.x);
    double thetaRestant = thetaWaypoint - ModuloByAngle(thetaWaypoint, ghostPosition.theta);
    double thetaArret = pow(ghostPosition.angularSpeed, 2) / (2 * MAX_ANGULAR_ACCEL);

    int shouldAccelerate = 0;
    int isDirectionPositive = thetaRestant > 0 ? 1 : 0;

    if (fabs(thetaRestant) < ANGLE_TOLERANCE) {
        ghostPosition.state = ADVANCING;
        ghostPosition.angularSpeed = 0;
        return;
    }

    if (isDirectionPositive) {
        shouldAccelerate = (ghostPosition.angularSpeed < MAX_ANGULAR_SPEED && thetaRestant > thetaArret) ? 1 : 0;
    } else {
        shouldAccelerate = (ghostPosition.angularSpeed > -MAX_ANGULAR_SPEED && fabs(thetaRestant) > thetaArret) ? 1 : 0;
    }

    if (shouldAccelerate) {
        ghostPosition.angularSpeed += (isDirectionPositive ? 1 : -1) * MAX_ANGULAR_ACCEL * deltaTime;
    } else if (fabs(thetaRestant) <= thetaArret || (!isDirectionPositive && ghostPosition.angularSpeed > 0) || (isDirectionPositive && ghostPosition.angularSpeed < 0)) {
        ghostPosition.angularSpeed -= (isDirectionPositive ? 1 : -1) * MAX_ANGULAR_ACCEL * deltaTime;
    }

    ghostPosition.angularSpeed = fmin(fmax(ghostPosition.angularSpeed, -MAX_ANGULAR_SPEED), MAX_ANGULAR_SPEED);
    ghostPosition.theta += ghostPosition.angularSpeed * deltaTime;
    ghostPosition.theta = ModuloByAngle(0, ghostPosition.theta);

    ghostPosition.angleToTarget = thetaRestant;
}

void AdvanceTowardsTarget(double currentTime) // Avancement lineaire du Ghost vers le waypoint
{
    double deltaTime = (currentTime - lastUpdateTime) / 1000.0;
    double distance = DistanceProjete(ghostPosition.x, ghostPosition.y, 
                                      ghostPosition.x + cos(ghostPosition.theta),  ghostPosition.y + sin(ghostPosition.theta),
                                      ghostPosition.targetX, ghostPosition.targetY); // distance par rapport au projetté sur la droite de direction

    if (distance < DISTANCE_TOLERANCE) {
        ghostPosition.state = IDLE;
        ghostPosition.linearSpeed = 0.0;
        return;
    }

    double accelDistance = (MAX_LINEAR_SPEED * MAX_LINEAR_SPEED - ghostPosition.linearSpeed * ghostPosition.linearSpeed) / (2 * MAX_LINEAR_ACCEL);
    double decelDistance = (ghostPosition.linearSpeed * ghostPosition.linearSpeed) / (2 * MAX_LINEAR_ACCEL);

    if (distance <= (decelDistance + DISTANCE_TOLERANCE)) {
        // Phase de deceleration
        ghostPosition.linearSpeed = (MAX_LINEAR_SPEED * distance) / decelDistance;
    } else if (distance > decelDistance + accelDistance) {
        ghostPosition.linearSpeed += MAX_LINEAR_ACCEL * deltaTime;
        ghostPosition.linearSpeed = fmin(ghostPosition.linearSpeed, MAX_LINEAR_SPEED);
    } else {
        double vMedian = sqrt(MAX_LINEAR_ACCEL * distance + ghostPosition.linearSpeed / 2);
        ghostPosition.linearSpeed += MAX_LINEAR_ACCEL * deltaTime;
        ghostPosition.linearSpeed = fmin(ghostPosition.linearSpeed, vMedian);
    }

    ghostPosition.distanceToTarget = distance;

    // Deplacer le robot vers le waypoint
    ghostPosition.x += ghostPosition.linearSpeed * cos(ghostPosition.theta) * deltaTime;
    ghostPosition.y += ghostPosition.linearSpeed * sin(ghostPosition.theta) * deltaTime;
}

void SendGhostData() {
    unsigned char ghostPayload[8];
    getBytesFromFloat(ghostPayload, 0, (float) ghostPosition.angleToTarget);
    getBytesFromFloat(ghostPayload, 4, (float) ghostPosition.distanceToTarget);

    UartEncodeAndSendMessage(GHOST_DATA, 8, ghostPayload);
}