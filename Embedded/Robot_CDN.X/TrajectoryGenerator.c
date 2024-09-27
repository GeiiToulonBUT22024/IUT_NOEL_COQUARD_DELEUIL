#include <math.h>
#include <stdlib.h>
#include "trajectoryGenerator.h"
#include "timer.h"
#include "Robot.h"
#include "utilities.h"
#include "UART_Protocol.h"
#include "QEI.h"

volatile GhostPosition ghostPosition;

double maxAngularSpeed = 2 * PI; // rad/s
double angularAccel = 2 * PI; // rad/s^2
double maxLinearSpeed = 1; // m/s
double minMaxLinenearSpeed = 0.2; // m/s
double linearAccel = 1; // m/s^2

void InitTrajectoryGenerator(void)
{
    ghostPosition.x = 0.0;
    ghostPosition.y = 0.0;
    ghostPosition.theta = 0.0;
    ghostPosition.linearSpeed = 0.0;
    ghostPosition.angularSpeed = 0.0;
    ghostPosition.targetX = 0.0;
    ghostPosition.targetY = 0.0;
    ghostPosition.angleToTarget = 0.0;
    ghostPosition.distanceToTarget = 0.0;
}

void UpdateTrajectory() // Mise a jour de la trajectoire en fonction de l'etat actuel par rapport au waypoint
{
    // Calcul de l'angle vers la cible (en rad)
    double targetAngle = atan2(ghostPosition.targetY - ghostPosition.y, ghostPosition.targetX - ghostPosition.x);
    // Calcul de l'angle a parcourir pour atteindre la cible
    double angleAParcourir = ModuloByAngle(ghostPosition.theta, targetAngle) - ghostPosition.theta;
    // Calcul de l'angle d'arret selon la vitesse angulaire actuelle
    double angleArret = ghostPosition.angularSpeed * ghostPosition.angularSpeed / (2 * angularAccel);
    // Calcul de la distance a parcourir pour atteindre la cible (en m)
    double distanceAParcourir = sqrt((ghostPosition.targetX - ghostPosition.x) * (ghostPosition.targetX - ghostPosition.x)
            + (ghostPosition.targetY - ghostPosition.y) * (ghostPosition.targetY - ghostPosition.y));
    // Calcul de la distance d'arret selon la vitesse lineaire actuelle
    double distanceArret = ghostPosition.linearSpeed * ghostPosition.linearSpeed / (2 * linearAccel);
    // Modulation de la vitesse lineaire maximale en fonction de l'angle a parcourir (rampe cosinusoidale)
    double vitesseLinMax = 0.5 * ((maxLinearSpeed + minMaxLinenearSpeed) + (maxLinearSpeed - minMaxLinenearSpeed) * cos(angleAParcourir));
    // Calcul du rayon maximal d'arret base sur les vitesses lineaire et angulaire
    double rayonArretMax = 0.5 * (maxLinearSpeed + minMaxLinenearSpeed) / maxAngularSpeed;

    // Gestion de la rotation pour atteindre l'angle cible
    if (angleAParcourir != 0 && distanceAParcourir > 0.01)
    {
        if (angleAParcourir > 0) // Si l'angle a parcourir est positif
        {
            if (angleAParcourir > angleArret) // Si l'angle a parcourir est superieur a l'angle d'arret
            {
                if (ghostPosition.angularSpeed >= maxAngularSpeed) // Si on a atteint la vitesse angulaire maximale
                {
                    // Vitesse angulaire maintenue
                }
                else
                {
                    // Acceleration avec saturation a la vitesse maximale
                    ghostPosition.angularSpeed = Min(ghostPosition.angularSpeed + angularAccel / FREQ_ECH_QEI, maxAngularSpeed);
                }
            }
            else // Si l'angle a parcourir est inferieur a l'angle d'arret
            {
                // Deceleration pour atteindre l'angle cible
                ghostPosition.angularSpeed = Max(ghostPosition.angularSpeed - angularAccel / FREQ_ECH_QEI, 0);
            }
        }
        else // Si l'angle a parcourir est negatif
        {
            if (abs(angleAParcourir) > angleArret) // Si l'angle a parcourir est superieur a l'angle d'arret
            {
                if (ghostPosition.angularSpeed <= -maxAngularSpeed) // Si on a atteint la vitesse angulaire maximale negative
                {
                    // Vitesse angulaire maintenue
                }
                else
                {
                    // Acceleration avec saturation a la vitesse maximale negative
                    ghostPosition.angularSpeed = Max(ghostPosition.angularSpeed - angularAccel / FREQ_ECH_QEI, -maxAngularSpeed);
                }
            }
            else // Si l'angle a parcourir est inferieur a l'angle d'arret
            {
                // Deceleration pour atteindre l'angle cible
                ghostPosition.angularSpeed = Min(ghostPosition.angularSpeed + angularAccel / FREQ_ECH_QEI, 0);
            }
        }

        // Mise a jour de l'orientation du ghost en fonction de la vitesse angulaire calculee
        ghostPosition.theta += ghostPosition.angularSpeed / FREQ_ECH_QEI;

        // Correction finale de l'orientation si la vitesse angulaire devient nulle (evite les erreurs d'arrondi)
        if (ghostPosition.angularSpeed == 0)
        {
            ghostPosition.theta = targetAngle;
        }
    }

    // Gestion du mouvement lineaire pour atteindre la cible
    if (distanceAParcourir != 0 && abs(angleAParcourir) < 0.5)
    {
        // Si la distance a parcourir est superieure a la distance d'arret
        if (distanceAParcourir > (distanceArret + ghostPosition.linearSpeed / FREQ_ECH_QEI))
        {
            if (ghostPosition.linearSpeed >= vitesseLinMax) // Si on a atteint la vitesse lineaire maximale calculee
            {
                ghostPosition.linearSpeed = Max(ghostPosition.linearSpeed - linearAccel / FREQ_ECH_QEI, vitesseLinMax);
            }
            else
            {
                // Acceleration avec saturation a la vitesse maximale calculee
                ghostPosition.linearSpeed = Min(ghostPosition.linearSpeed + linearAccel / FREQ_ECH_QEI, vitesseLinMax);
            }
        }
        else // Si la distance a parcourir est inferieure a la distance d'arret
        {
            // Deceleration pour atteindre la cible
            ghostPosition.linearSpeed = Max(ghostPosition.linearSpeed - linearAccel / FREQ_ECH_QEI, 0);
        }

        // Si la distance a parcourir devient tres faible (fin du deplacement)
        if (distanceAParcourir <= rayonArretMax)
        {
            // Correction finale de la position du ghost pour eviter les erreurs d'arrondi
            ghostPosition.x = ghostPosition.targetX;
            ghostPosition.y = ghostPosition.targetY;
            ghostPosition.linearSpeed = 0;
        }

        // Mise a jour des consignes de vitesse pour le robot
        robotState.consigneLin = ghostPosition.linearSpeed;
        robotState.consigneAng = ghostPosition.angularSpeed;
    }

    // Mise a jour de la position du ghost en fonction de la vitesse lineaire
    double deltaParcouru = ghostPosition.linearSpeed / FREQ_ECH_QEI;
    ghostPosition.x += deltaParcouru * cos(ghostPosition.theta);
    ghostPosition.y += deltaParcouru * sin(ghostPosition.theta);
}

void SendGhostData()
{
    unsigned char ghostPayload[24];
    getBytesFromFloat(ghostPayload, 0, (float) ghostPosition.angleToTarget);
    getBytesFromFloat(ghostPayload, 4, (float) ghostPosition.distanceToTarget);
    getBytesFromFloat(ghostPayload, 8, (float) ghostPosition.theta);
    getBytesFromFloat(ghostPayload, 12, (float) ghostPosition.angularSpeed);
    getBytesFromFloat(ghostPayload, 16, (float) ghostPosition.x);
    getBytesFromFloat(ghostPayload, 20, (float) ghostPosition.y);
    UartEncodeAndSendMessage(GHOST_DATA, 24, ghostPayload);
}