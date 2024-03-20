#include <math.h>
#include "trajectoryGenerator.h"
#include "timer.h"
#include "Robot.h"
#include "utilities.h"

volatile GhostPosition ghostPosition;
static unsigned long lastUpdateTime = 0;


void InitTrajectoryGenerator(void)
{
    ghostPosition.x = 0.0;
    ghostPosition.y = 0.0;
    ghostPosition.theta = 0.0;
    ghostPosition.linearSpeed = 0.0;
    ghostPosition.angularSpeed = 0.0;
    ghostPosition.targetX = 0.0;
    ghostPosition.targetY = 0.0; 
    ghostPosition.state = TrajectoryState.IDLE;
}

void UpdateTrajectory(double currentTime) // Mise a jour de la trajectoire en fonction de l'etat actuel
{
    switch (ghostPosition.state)
    {
        case IDLE:
            ghostPosition.linearSpeed = 0.0;
            ghostPosition.angularSpeed = 0.0;

            if (ghostPosition.targetX != 0.0 || ghostPosition.targetY != 0.0) // Verifier si une nouvelle cible est definie
            {
                // Determiner l'angle actuel du robot par rapport a la cible
                double angleToTarget = atan2(ghostPosition.targetY - ghostPosition.y, ghostPosition.targetX - ghostPosition.x);
                double angleDifference = ModuloByAngle(ghostPosition.theta, angleToTarget - ghostPosition.theta);

                // Si le robot n'est pas axe vers la cible, commencer par tourner
                if (fabs(angleDifference) > ANGLE_TOLERANCE)
                {
                    ghostPosition.state = ROTATING;
                }
                else
                {
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
    SendConsigne(); // calculer vitesse moteur gauche et droit et l'envoyer comme consigne au PID (en fonction de angular speed et linear speed)
}

void RotateTowardsTarget(double currentTime) // Orientation du Ghost vers le waypoint
{
    double deltaTime = (currentTime - lastUpdateTime) / 1000.0;
    double thetaWaypoint = atan2(ghostPosition.targetY - ghostPosition.y, ghostPosition.targetX - ghostPosition.x);
    double thetaRestant = thetaWaypoint - ModuloByAngle(thetaWaypoint, ghostPosition.theta);
    double thetaArret = pow(ghostPosition.angularSpeed, 2) / (2 * MAX_ANGULAR_ACCEL);

    int shouldAccelerate = 0;
    int isDirectionPositive = thetaRestant > 0 ? 1 : 0;

    if (fabs(thetaRestant) < ANGLE_TOLERANCE)
    {
        ghostPosition.state = ADVANCING;
        ghostPosition.angularSpeed = 0;
        return;
    }

    if (isDirectionPositive)
    {
        shouldAccelerate = (ghostPosition.angularSpeed < MAX_ANGULAR_SPEED && thetaRestant > thetaArret) ? 1 : 0;
    }
    else
    {
        shouldAccelerate = (ghostPosition.angularSpeed > -MAX_ANGULAR_SPEED && fabs(thetaRestant) > thetaArret) ? 1 : 0;
    }

    if (shouldAccelerate)
    {
        ghostPosition.angularSpeed += (isDirectionPositive ? 1 : -1) * MAX_ANGULAR_ACCEL * deltaTime;
    }
    else if (fabs(thetaRestant) <= thetaArret || (!isDirectionPositive && ghostPosition.angularSpeed > 0) || (isDirectionPositive && ghostPosition.angularSpeed < 0))
    {
        ghostPosition.angularSpeed -= (isDirectionPositive ? 1 : -1) * MAX_ANGULAR_ACCEL * deltaTime;
    }

    ghostPosition.angularSpeed = fmin(fmax(ghostPosition.angularSpeed, -MAX_ANGULAR_SPEED), MAX_ANGULAR_SPEED);
    ghostPosition.theta += ghostPosition.angularSpeed * deltaTime;
    ghostPosition.theta = ModuloByAngle(0, ghostPosition.theta);
}

void AdvanceTowardsTarget(double currentTime) // Avancement lineaire du Ghost vers le waypoint
{
    double deltaTime = (currentTime - lastUpdateTime) / 1000.0;

    // Direction du waypoint par rapport a la position actuelle du robot
    double waypointDirectionX = ghostPosition.x * cos(ghostPosition.theta); // A changer (projetté sur la direction)
    double waypointDirectionY = ghostPosition.y * sin(ghostPosition.theta); // A changer (projetté sur la direction)

    // Calcul de la distance entre le robot et la projection du waypoint sur son axe de deplacement
    double distance = DistancePointToSegment(controle.targetX, controle.targetY, ghostPosition.x, ghostPosition.y, waypointDirectionX, waypointDirectionY);

    if (distance < DISTANCE_TOLERANCE)
    {
        controle.state = IDLE;
        ghostPosition.linearSpeed = 0.0;
        return;
    }

    double decelDistance = (MAX_LINEAR_SPEED * MAX_LINEAR_SPEED) / (2 * MAX_LINEAR_ACCEL);

    // Decider de la phase : acceleration, maintien ou deceleration
    if (distance <= decelDistance)
    {
        // Phase de deceleration
        ghostPosition.linearSpeed = sqrt(2 * MAX_LINEAR_ACCEL * distance);
    }
    else
    {
        if (ghostPosition.linearSpeed < MAX_LINEAR_SPEED)
        {
            // Phase d'acceleration
            ghostPosition.linearSpeed += MAX_LINEAR_ACCEL * deltaTime;
            ghostPosition.linearSpeed = fmin(ghostPosition.linearSpeed, MAX_LINEAR_SPEED);
        }
        else
        {
            // Phase de maintien de la vitesse
            ghostPosition.linearSpeed = MAX_LINEAR_SPEED;
        }
    }

    // Deplacer le robot vers le waypoint
    ghostPosition.x += ghostPosition.linearSpeed * cos(ghostPosition.theta) * deltaTime;
    ghostPosition.y += ghostPosition.linearSpeed * sin(ghostPosition.theta) * deltaTime;

}


// To Suppr
void CalculateGhostSpeed() // Calcul de la position theorique en fonction de la cible
{
    double deltaX = ghostposition.targetX - ghostPosition.x; 
    double deltaY = ghostposition.targetY - ghostPosition.y; 
    double distanceTotale = sqrt(deltaX * deltaX + deltaY * deltaY); // Distance totale a parcourir (hypotenuse)

    double tempsAccelDecel, distanceAccelDecel, distanceVitesseMax;
    
    tempsAccelDecel = (2.0 * MAX_LINEAR_SPEED) / MAX_LINEAR_ACCEL; // Temps total necessaire pour accelerer de 0 a vitesse max puis decelerer a 0 (x2 car 2 phases constantes)
    distanceAccelDecel = (MAX_LINEAR_ACCEL * tempsAccelDecel * tempsAccelDecel) / 4.0; // Distance totale parcourue pendant l'acceleration et la deceleration

    /* EXPLICATIONS
    d = 0.5 * a * t^2 pour calculer la distance parcourue pendant une seule phase (acceleration ou deceleration)
    oÃ¹ d est la distance, a est l'acceleration (MAX_LINEAR_ACCEL) et t est le temps
    
    Comme tempsAccelDecel est le temps total pour accelerer de 0 a MAX_LINEAR_SPEED puis decelerer a 0, le temps pour une seule phase est de tempsAccelDecel / 2

    Donc la distance pour une seule phase : d_une_phase = 0.5 * MAX_LINEAR_ACCEL * (tempsAccelDecel/2)^2

    Simplification : d_une_phase = 0.5 * MAX_LINEAR_ACCEL * (tempsAccelDecel^2 / 4) <=> d_une_phase = (MAX_LINEAR_ACCEL * tempsAccelDecel^2) / 8

    Comme il y a deux phases, la distance totale = 2 * d_une_phase : d_totale = 2 * ((MAX_LINEAR_ACCEL * tempsAccelDecel^2) / 8)

    Ce qui peut Ãªtre simplifie : d_totale : (MAX_LINEAR_ACCEL * tempsAccelDecel * tempsAccelDecel) / 4
    */

    if (distanceTotale <= distanceAccelDecel) // Si la distance a parcourir est inferieure a la distance d'acceleration/deceleration
    {
        double tempsAccel = sqrt(distanceTotale / MAX_LINEAR_ACCEL); // Calcul du temps necessaire pour accelerer sur la distance totale
        ghostPosition.linearSpeed = MAX_LINEAR_ACCEL * tempsAccel; // Ajustement de la vitesse lineaire pour l'acceleration sur la distance totale
    }
    else // Si la distance a parcourir permet d'atteindre la vitesse maximale
    {
        distanceVitesseMax = distanceTotale - distanceAccelDecel; // Calcul de la distance parcourue a vitesse maximale
        ghostPosition.linearSpeed = MAX_LINEAR_SPEED; // Ajustement de la vitesse lineaire a la vitesse maximale
    }
}

// To Suppr
void UpdateGhostPosition(void) // Mise a jour de la position du ghost en fonction de la trajectoire calculee
{
    double currentTime = timestamp;
    double deltaTime = (currentTime - lastUpdateTime) / 1000.0;

    ghostPosition.x += ghostPosition.linearSpeed * cos(ghostPosition.theta) * deltaTime;
    ghostPosition.y += ghostPosition.linearSpeed * sin(ghostPosition.theta) * deltaTime;

    lastUpdateTime = currentTime;
}

