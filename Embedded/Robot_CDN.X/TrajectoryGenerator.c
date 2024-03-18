#include <math.h>
#include "trajectoryGenerator.h"
#include "timer.h"

static GhostPosition ghostPosition;
static TrajectoryControl controle;
static unsigned long lastUpdateTime = 0;

double ModuloByAngle(double angleToCenterAround, double angleToCorrect) // Fonction pour normaliser un angle entre -PI et PI
{
    int decalageNbTours = (int) round((angleToCorrect - angleToCenterAround) / (2 * M_PI));
    double thetaDest = angleToCorrect - decalageNbTours * 2 * M_PI;
    return thetaDest;
}

void InitTrajectoryGenerator(void)
{
    ghostPosition.x = 0.0;
    ghostPosition.y = 0.0;
    ghostPosition.theta = 0.0;
    ghostPosition.linearSpeed = 0.0;
    ghostPosition.angularSpeed = 0.0;

    controle.state = IDLE;
    controle.targetX = 0.0;
    controle.targetY = 0.0;
    controle.targetTheta = 0.0;
}

void CalculateGhostPosition(double targetX, double targetY, double targetTheta) // Calcul de la position théorique en fonction de la cible
{
    double deltaX = targetX - ghostPosition.x; 
    double deltaY = targetY - ghostPosition.y; 
    double distanceTotale = sqrt(deltaX * deltaX + deltaY * deltaY); // Distance totale à parcourir (hypoténuse)

    double tempsAccelDecel, distanceAccelDecel, distanceVitesseMax;
    
    tempsAccelDecel = (2 * MAX_LINEAR_SPEED) / MAX_LINEAR_ACCEL; // Temps total necessaire pour accélérer de 0 à vitesse max puis décélérer à 0 (x2 car 2 phases constantes)

    distanceAccelDecel = (MAX_LINEAR_ACCEL * tempsAccelDecel * tempsAccelDecel) / 4; // Distance totale parcourue pendant l'accélération et la décélération

    /* EXPLICATIONS
    d = 0.5 * a * t^2 pour calculer la distance parcourue pendant une seule phase (accélération ou décélération)
    où d est la distance, a est l'accélération (MAX_LINEAR_ACCEL) et t est le temps
    
    Comme tempsAccelDecel est le temps total pour accélérer de 0 à MAX_LINEAR_SPEED puis décélérer à 0, le temps pour une seule phase est de tempsAccelDecel / 2

    Donc la distance pour une seule phase : d_une_phase = 0.5 * MAX_LINEAR_ACCEL * (tempsAccelDecel/2)^2

    Simplification : d_une_phase = 0.5 * MAX_LINEAR_ACCEL * (tempsAccelDecel^2 / 4) <=> d_une_phase = (MAX_LINEAR_ACCEL * tempsAccelDecel^2) / 8

    Comme il y a deux phases, la distance totale = 2 * d_une_phase : d_totale = 2 * ((MAX_LINEAR_ACCEL * tempsAccelDecel^2) / 8)

    Ce qui peut être simplifié : d_totale : (MAX_LINEAR_ACCEL * tempsAccelDecel * tempsAccelDecel) / 4
    */

    if (distanceTotale <= distanceAccelDecel) // Si la distance à parcourir est inférieure à la distance d'accélération/décélération
    {
        double tempsAccel = sqrt(distanceTotale / MAX_LINEAR_ACCEL); // Calcul du temps nécessaire pour accélérer sur la distance totale
        ghostPosition.linearSpeed = MAX_LINEAR_ACCEL * tempsAccel; // Ajustement de la vitesse linéaire pour l'accélération sur la distance totale
    }
    else // Si la distance à parcourir permet d'atteindre la vitesse maximale
    {
        distanceVitesseMax = distanceTotale - distanceAccelDecel; // Calcul de la distance parcourue à vitesse maximale
        ghostPosition.linearSpeed = MAX_LINEAR_SPEED; // Ajustement de la vitesse linéaire à la vitesse maximale
    }
}

void UpdateGhostPosition(void) // Mise a jour de la position du ghost en fonction de la trajectoire calcul�e
{
    double currentTime = timestamp;
    double deltaTime = (currentTime - lastUpdateTime) / 1000.0;

    ghostPosition.x += ghostPosition.linearSpeed * cos(ghostPosition.theta) * deltaTime;
    ghostPosition.y += ghostPosition.linearSpeed * sin(ghostPosition.theta) * deltaTime;

    lastUpdateTime = currentTime;
}

void UpdateTrajectory(double currentTime) // Mise a jour de la trajectoire en fonction de l'etat actuel
{
    switch (controle.state)
    {
        case IDLE:
            ghostPosition.linearSpeed = 0.0;
            ghostPosition.angularSpeed = 0.0;

            
            if (controle.targetX != 0.0 || controle.targetY != 0.0) // Verifier si une nouvelle cible est definie
            {
                // Determiner l'angle actuel du robot par rapport a la cible
                double angleToTarget = atan2(controle.targetY - ghostPosition.y, controle.targetX - ghostPosition.x);
                double angleDifference = ModuloByAngle(ghostPosition.theta, angleToTarget - ghostPosition.theta);

                // Si le robot n'est pas axe vers la cible, commencer par tourner
                if (fabs(angleDifference) > ANGLE_TOLERANCE)
                {
                    controle.state = ROTATING;
                }
                else
                {
                    // Si le robot est deja oriente vers la cible, passer directement à l'avancement
                    controle.state = ADVANCING;
                }
            }
            break;

        case ROTATING:
            RotateTowardsTarget(currentTime);
            break;

        case ADVANCING:
            AdvanceTowardsTarget(currentTime);
            break;
    }
}

void RotateTowardsTarget(double currentTime) // Orientation du Ghost vers le waypoint
{
    double deltaTime = (currentTime - lastUpdateTime) / 1000.0;
    double thetaWaypoint = atan2(controle.targetY - ghostPosition.y, controle.targetX - ghostPosition.x);
    double thetaRestant = thetaWaypoint - ModuloByAngle(thetaWaypoint, ghostPosition.theta);
    double thetaArret = pow(ghostPosition.angularSpeed, 2) / (2 * MAX_ANGULAR_ACCEL);

    int shouldAccelerate = 0;
    int isDirectionPositive = thetaRestant > 0 ? 1 : 0;

    if (fabs(thetaRestant) < ANGLE_TOLERANCE)
    {
        controle.state = ADVANCING;
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

double DistancePointToSegment(double ptX, double ptY, double ptSeg1X, double ptSeg1Y, double ptSeg2X, double ptSeg2Y)
{
    // Calcul des vecteurs entre le point et le premier point du segment
    double A = ptX - ptSeg1X;
    double B = ptY - ptSeg1Y;

    // Calcul du vecteur directeur du segment
    double C = ptSeg2X - ptSeg1X;
    double D = ptSeg2Y - ptSeg1Y;

    double dot = A * C + B * D; // Calcul du produit scalaire des vecteurs (A,B) et (C,D)
    double len_sq = C * C + D * D; // Calcul du carré de la longueur du segment

    double param = -1; // Calcul du paramètre qui nous indique le point le plus proche sur le segment par rapport au point donné
    if (len_sq != 0) 
        param = dot / len_sq; // Pour éviter la division par zéro

    double xx, yy;

    // Si param < 0, cela signifie que le point projeté tombe en dehors du segment, plus proche de ptSeg1
    if (param < 0)
    {
        xx = ptSeg1X;
        yy = ptSeg1Y;
    }
    // Si param > 1, le point projeté tombe également en dehors du segment, mais plus proche de ptSeg2
    else if (param > 1)
    {
        xx = ptSeg2X;
        yy = ptSeg2Y;
    }
    // Si 0 <= param <= 1, le point projeté tombe sur le segment
    else
    {
        xx = ptSeg1X + param * C;
        yy = ptSeg1Y + param * D;
    }

    // Calcul de la distance entre le point donné et le point le plus proche sur le segment
    double dx = ptX - xx;
    double dy = ptY - yy;
    return sqrt(dx * dx + dy * dy);
}

void AdvanceTowardsTarget(double currentTime) // Avancement lineaire du Ghost vers le waypoint
{
    double deltaTime = (currentTime - lastUpdateTime) / 1000.0;

    // Direction du waypoint par rapport a la position actuelle du robot
    double waypointDirectionX = ghostPosition.x + cos(ghostPosition.theta);
    double waypointDirectionY = ghostPosition.y + sin(ghostPosition.theta);

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

    lastUpdateTime = currentTime;
}