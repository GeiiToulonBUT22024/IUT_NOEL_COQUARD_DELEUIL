#include <math.h>
//#include <stdlib.h>
#include "GhostManager.h"
#include "timer.h"
#include "Robot.h"
#include "utilities.h"
#include "UART_Protocol.h"
#include "QEI.h"

extern unsigned long timestamp;

volatile GhostPosition ghostPosition;

double maxAngularSpeed = 0.2; // rad/s
double angularAccel = 0.2; // rad/s^2
double maxLinearSpeed = 1; // m/s
double minMaxLinenearSpeed = 0.2; // m/s
double linearAccel = 0.5; // m/s^2

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
}

void UpdateTrajectory() // Mise a jour de la trajectoire en fonction de l'etat actuel par rapport au waypoint
{
    // Target -> le waypoint : c'est où on veut aller
    double thetaTarget = atan2(ghostPosition.targetY - ghostPosition.y, ghostPosition.targetX - ghostPosition.x);
    // Theta entre le robot et où on veut aller
    double thetaRestant = ModuloByAngle(ghostPosition.theta, thetaTarget) - ghostPosition.theta;
    ghostPosition.angleToTarget = thetaRestant;
    // Theta à partir duquel on considère que c'est good
    double thetaArret = ghostPosition.angularSpeed * ghostPosition.angularSpeed / (2 * angularAccel);
    // Pas d'angle à ajouter
    double incrementAng = ghostPosition.angularSpeed / FREQ_ECH_QEI;
    double incremntLin = ghostPosition.linearSpeed / FREQ_ECH_QEI;

    double distanceArret = ghostPosition.linearSpeed * ghostPosition.linearSpeed / (2 * linearAccel);

    double distanceRestante = sqrt((ghostPosition.targetX - ghostPosition.x) * (ghostPosition.targetX - ghostPosition.x)
            + (ghostPosition.targetY - ghostPosition.y) * (ghostPosition.targetY - ghostPosition.y));
    ghostPosition.distanceToTarget = distanceRestante;

    if (ghostPosition.angularSpeed < 0) thetaArret = -thetaArret;

    if (((thetaArret >= 0 && thetaRestant >= 0) || (thetaArret <= 0 && thetaRestant <= 0)) && (Abs(thetaRestant) >= Abs(thetaArret))) {
        // on accélère en rampe saturée 
        if (thetaRestant > 0) {
            ghostPosition.angularSpeed = Min(ghostPosition.angularSpeed + angularAccel / FREQ_ECH_QEI, maxAngularSpeed);
        } else if (thetaRestant < 0) {
            ghostPosition.angularSpeed = Max(ghostPosition.angularSpeed - angularAccel / FREQ_ECH_QEI, -maxAngularSpeed);
        } else {
            ghostPosition.angularSpeed = 0;
        }
    } else {
        //on freine en rampe saturée
        if (thetaRestant > 0) {
            ghostPosition.angularSpeed = Max(ghostPosition.angularSpeed - angularAccel / FREQ_ECH_QEI, 0);
        } else if (thetaRestant < 0) {
            ghostPosition.angularSpeed = Min(ghostPosition.angularSpeed + angularAccel / FREQ_ECH_QEI, 0);
        } else {
            ghostPosition.angularSpeed = 0;
        }

        if (Abs(thetaRestant) < Abs(incrementAng)) {
            incrementAng = thetaRestant;
        }
    }
    
    ghostPosition.theta += incrementAng;
    robotState.consigneVitesseAngulaire = ghostPosition.angularSpeed;
    
    if(ghostPosition.angularSpeed == 0 ) //TODO rajouter condition sur angle à parcourir petit
        ghostPosition.theta = thetaTarget;

    
    
//    if ((distanceRestante != 0) && (Modulo2PIAngleRadian(thetaRestant) < 0.01)) {
//        if (((distanceArret    >= 0 && distanceRestante >= 0) || (distanceArret <= 0 && distanceRestante <= 0)) && abs(distanceRestante) >= abs(distanceArret)) {
//            if (distanceRestante > 0) {
//                ghostPosition.linearSpeed = Min(ghostPosition.linearSpeed + linearAccel / FREQ_ECH_QEI, maxLinearSpeed);
//            }
//            else if (distanceRestante < 0){
//                ghostPosition.linearSpeed = Max(ghostPosition.linearSpeed - linearAccel / FREQ_ECH_QEI, -maxLinearSpeed);
//            }
//            else 
//                ghostPosition.linearSpeed = 0;
//        }
//        else {
//            if (distanceRestante > 0) {
//                ghostPosition.linearSpeed = Max(ghostPosition.linearSpeed - linearAccel / FREQ_ECH_QEI, 0);
//            }
//            else if (distanceRestante < 0){
//                ghostPosition.linearSpeed = Min(ghostPosition.linearSpeed + linearAccel / FREQ_ECH_QEI, 0);
//            }
//            else ghostPosition.linearSpeed = 0;
//        }
//        
//        if (abs(distanceRestante) < abs(incremntLin)) {
//            incremntLin = distanceRestante;
//        }
//    }
//    ghostPosition.x += incremntLin * cos(ghostPosition.theta);
//    ghostPosition.y += incremntLin * sin(ghostPosition.theta);
//    robotState.consigneVitesseLineaire = ghostPosition.linearSpeed;
   
    SendGhostData();
}

void SendGhostData() {
    unsigned char ghostPayload[28];
    getBytesFromInt32(ghostPayload, 0, timestamp);
    getBytesFromFloat(ghostPayload, 4, (float) ghostPosition.angleToTarget);
    getBytesFromFloat(ghostPayload, 8, (float) ghostPosition.distanceToTarget);
    getBytesFromFloat(ghostPayload, 12, (float) ghostPosition.theta);
    getBytesFromFloat(ghostPayload, 16, (float) ghostPosition.angularSpeed);
    getBytesFromFloat(ghostPayload, 20, (float) ghostPosition.x);
    getBytesFromFloat(ghostPayload, 24, (float) ghostPosition.y);
    UartEncodeAndSendMessage(GHOST_DATA, 28, ghostPayload);
}