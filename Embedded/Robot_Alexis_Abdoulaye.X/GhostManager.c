#include <math.h>
#include "GhostManager.h"
#include "timer.h"
#include "utilities.h"
#include "Robot.h"
#include "UART_Protocol.h"

volatile GhostPosition ghostPosition;
static unsigned long lastUpdateTime = 0;

void InitTrajectoryGenerator(void) {
    ghostPosition.x = 0.0;
    ghostPosition.y = 0.0;
    ghostPosition.theta = 0.0;
    ghostPosition.linearSpeed = 0.0;
    ghostPosition.angularSpeed = 0.0;
    ghostPosition.targetX = 0.0;
    ghostPosition.targetY = 1.0;
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
                double angleDifference = moduloByAngle(ghostPosition.theta, angleToTarget - ghostPosition.theta);

                // Si le robot n'est pas axe vers la cible, commencer par tourner
                if (fabs(angleDifference) > ANGLE_TOLERANCE) {
                    ghostPosition.state = ROTATING;
                } 
                
                else {
                    // Si le robot est deja oriente vers la cible, passer directement a l'avancement
                    ghostPosition.state = ADVANCING;
                }
            }
            break;

        case ROTATING:
            rotationTarget(timestamp);
            break;

        case ADVANCING:
            // todo 
            break;
    }

    lastUpdateTime = timestamp;
    //    robotState.consigneLin = ghostPosition.linearSpeed;
    //    robotState.consigneAng = ghostPosition.angularSpeed;
}

void rotationTarget(double currentTime) {
    double deltaTime = (timestamp - lastUpdateTime) / 1000.0;
    double angleToTarget = atan2(ghostPosition.targetY - ghostPosition.y, ghostPosition.targetX - ghostPosition.x);
    double angleDifference = moduloByAngle(ghostPosition.theta, angleToTarget - ghostPosition.theta);
    double thetaArret = pow(ghostPosition.angularSpeed, 2) / (2 * MAX_ANGULAR_ACCEL);

    if (fabs(angleDifference) < ANGLE_TOLERANCE) {
        ghostPosition.state = ADVANCING;
        ghostPosition.angularSpeed = 0;
    }

    else {
        // Soit thetaRest > 0 (rota positive) && accel 
        if (angleDifference > 0 && ghostPosition.angularSpeed <= MAX_ANGULAR_SPEED && angleDifference > thetaArret) {
            // accel
            ghostPosition.angularSpeed += MAX_ANGULAR_ACCEL * deltaTime;

        }

        // Soit thetaRest > 0 (rota positive) && deccel 
        if (angleDifference > 0 && ghostPosition.angularSpeed <= MAX_ANGULAR_SPEED && angleDifference < thetaArret) {
            // decel
            ghostPosition.angularSpeed += -MAX_ANGULAR_ACCEL * deltaTime;
            
        }

        // Soit thetaRest < 0 (rota negative) && accel 
        if (angleDifference < 0 && ghostPosition.angularSpeed >= -MAX_ANGULAR_SPEED && angleDifference > thetaArret) {
            // accel
            ghostPosition.angularSpeed -= MAX_ANGULAR_ACCEL * deltaTime;
        }

        // Soit thetaRest < 0 (rota negative) && deccel 
        if (angleDifference < 0 && ghostPosition.angularSpeed >= -MAX_ANGULAR_SPEED && angleDifference < thetaArret) {
            // decel
            ghostPosition.angularSpeed -= -MAX_ANGULAR_ACCEL * deltaTime;
        }
    }

    if (ghostPosition.angularSpeed > MAX_ANGULAR_SPEED) ghostPosition.angularSpeed = MAX_ANGULAR_SPEED;

    ghostPosition.angularSpeed = fmin(fmax(ghostPosition.angularSpeed, -MAX_ANGULAR_SPEED), MAX_ANGULAR_SPEED);
    ghostPosition.theta += ghostPosition.angularSpeed * deltaTime;
    ghostPosition.theta = moduloByAngle(0, ghostPosition.theta);

    ghostPosition.angleToTarget = angleDifference;
}

void SendGhostData() {
    unsigned char positionPayload[8];
    getBytesFromInt32(positionPayload, 0, timestamp);
    getBytesFromFloat(positionPayload, 4, (float) (ghostPosition.angularSpeed));
    UartEncodeAndSendMessage(GHOST_DATA, 8, positionPayload);
}