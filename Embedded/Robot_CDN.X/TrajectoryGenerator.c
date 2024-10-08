#include <math.h>
#include <stdlib.h>
#include "trajectoryGenerator.h"
#include "timer.h"
#include "Robot.h"
#include "utilities.h"
#include "UART_Protocol.h"
#include "QEI.h"

volatile GhostPosition_t ghostPosition;

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
    // Target -> le waypoint : c'est où on veut aller
    double thetaTarget = atan2(ghostPosition.targetY - ghostPosition.y , ghostPosition.targetX - ghostPosition.x);
    // Theta entre le robot et où on veut aller
    double thetaRestant = ModuloByAngle(ghostPosition.theta,thetaTarget) - ghostPosition.theta;
    // Theta à partir duquel on considère que c'est good
    double thetaArret = ghostPosition.angularSpeed * ghostPosition.angularSpeed / 2 * angularAccel;
    // Pas d'angle à ajouter
    double incrementAng = ghostPosition.angularSpeed / FREQ_ECH_QEI;
    
    if (((thetaArret >= 0 && thetaRestant >=0 ) || (thetaArret <=0 && thetaRestant <= 0)) && abs(thetaRestant)>= abs(thetaArret)){
        // on accélère en rampe saturée 
        if (thetaRestant > 0 ){
            ghostPosition.angularSpeed = Min(ghostPosition.angularSpeed + angularAccel / FREQ_ECH_QEI, maxAngularSpeed);
        }
        else if (thetaRestant < 0){
            ghostPosition.angularSpeed = Max(ghostPosition.angularSpeed - angularAccel / FREQ_ECH_QEI, -maxAngularSpeed);
        }
        else {
            ghostPosition.angularSpeed = 0;
        }
    }
    else{
        //on freine en rampe saturée
        if (thetaRestant > 0){
            ghostPosition.angularSpeed = Max(ghostPosition.angularSpeed - angularAccel / FREQ_ECH_QEI, 0);
        }
        else if (thetaRestant < 0){
            ghostPosition.angularSpeed = Min(ghostPosition.angularSpeed + angularAccel / FREQ_ECH_QEI, 0);
        }
        else {
            ghostPosition.angularSpeed = 0;
        }
        
        if (abs(thetaRestant)< abs(incrementAng)){
            incrementAng = thetaRestant;
        }
    }
    
    ghostPosition.theta += incrementAng;
    robotState.consigneAng = ghostPosition.angularSpeed;
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