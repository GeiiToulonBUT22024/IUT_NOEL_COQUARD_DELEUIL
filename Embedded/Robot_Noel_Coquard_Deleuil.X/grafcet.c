#include <stdio.h>
#include <stdlib.h>
#include <xc.h>
#include "ChipConfig.h"
#include "IO.h"
#include "timer.h"
#include "Robot.h"
#include "PWM.h"
#include "main.h"
#include "grafcet.h"

unsigned char stateRobot;
int compteur_extreme = 0;
int compteur_marche_arriere = 0;

void OperatingSystemLoop(void) {
    switch (stateRobot) {
        case STATE_ATTENTE:
            timestamp = 0;
            PWMSetSpeedConsigne(0, MOTEUR_DROIT);
            PWMSetSpeedConsigne(0, MOTEUR_GAUCHE);
            stateRobot = STATE_ATTENTE_EN_COURS;

        case STATE_ATTENTE_EN_COURS:
            if (timestamp > 1000)
                stateRobot = STATE_AVANCE;
            break;

        case STATE_RALENTIT:
            PWMSetSpeedConsigne(30, MOTEUR_DROIT);
            PWMSetSpeedConsigne(30, MOTEUR_GAUCHE);
            stateRobot = STATE_RALENTIT_EN_COURS;
            break;
        case STATE_RALENTIT_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_AVANCE:
            PWMSetSpeedConsigne(40, MOTEUR_DROIT);
            PWMSetSpeedConsigne(40, MOTEUR_GAUCHE);
            stateRobot = STATE_AVANCE_EN_COURS;
            break;
        case STATE_AVANCE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_GAUCHE:
            PWMSetSpeedConsigne(9, MOTEUR_DROIT);
            PWMSetSpeedConsigne(-4, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_GAUCHE_EN_COURS;
            break;
        case STATE_TOURNE_GAUCHE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_DROITE:
            PWMSetSpeedConsigne(-4, MOTEUR_DROIT);
            PWMSetSpeedConsigne(9, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_DROITE_EN_COURS;
            break;
        case STATE_TOURNE_DROITE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_SUR_PLACE_GAUCHE:
            PWMSetSpeedConsigne(15, MOTEUR_DROIT);
            PWMSetSpeedConsigne(-15, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS;
            break;
        case STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_SUR_PLACE_DROITE:
            PWMSetSpeedConsigne(-10, MOTEUR_DROIT);
            PWMSetSpeedConsigne(10, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_SUR_PLACE_DROITE_EN_COURS;
            break;
        case STATE_TOURNE_SUR_PLACE_DROITE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_VITE_DROITE:
            PWMSetSpeedConsigne(-6, MOTEUR_DROIT);
            PWMSetSpeedConsigne(12, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_VITE_DROITE_EN_COURS;
            break;
        case STATE_TOURNE_VITE_DROITE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_TOURNE_VITE_GAUCHE:
            PWMSetSpeedConsigne(12, MOTEUR_DROIT);
            PWMSetSpeedConsigne(-6, MOTEUR_GAUCHE);
            stateRobot = STATE_TOURNE_VITE_GAUCHE_EN_COURS;
            break;
        case STATE_TOURNE_VITE_GAUCHE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        case STATE_MARCHE_ARRIERE:
            PWMSetSpeedConsigne(-10, MOTEUR_DROIT);
            PWMSetSpeedConsigne(-10, MOTEUR_GAUCHE);
            stateRobot = STATE_MARCHE_ARRIERE_EN_COURS;
            break;
        case STATE_MARCHE_ARRIERE_EN_COURS:
            SetNextRobotStateInAutomaticMode();
            break;

        default:
            stateRobot = STATE_ATTENTE;
            break;
    }
}

unsigned char nextStateRobot = 0;

void SetNextRobotStateInAutomaticMode() {
    unsigned char positionObstacle = PAS_D_OBSTACLE;

    //D�termination de la position des obstacles en fonction des t�l�m�tres
    if (compteur_extreme >= 3000) {
        if (robotState.distanceTelemetreGauche <= robotState.distanceTelemetreDroit)
            positionObstacle = OBSTACLE_DEVANT_GAUCHE;
        else if (robotState.distanceTelemetreGauche >= robotState.distanceTelemetreDroit)
            positionObstacle = OBSTACLE_DEVANT_DROITE;
    } else if (robotState.distanceTelemetreDroit > 30 &&
            robotState.distanceTelemetreCentre > 40 &&
            robotState.distanceTelemetreGauche > 30 &&           
            robotState.distanceTelemetreExtremeDroite > 20 &&
            robotState.distanceTelemetreExtremeGauche > 20) { // Tous les capteurs voient une distance suffisante, on passe alros en pleine vitesse
        positionObstacle = PAS_D_OBSTACLE;
   
    }
    else if (robotState.distanceTelemetreExtremeDroite < 15 &&
            robotState.distanceTelemetreExtremeGauche < 15 &&    
            robotState.distanceTelemetreCentre > 20) { //
        positionObstacle = PAS_OBSTACLE_PROCHE;
       
    } 
    else if (robotState.distanceTelemetreDroit < 15 &&
            robotState.distanceTelemetreCentre > 20 &&
            robotState.distanceTelemetreGauche < 15 &&           
            robotState.distanceTelemetreExtremeDroite < 15 &&
            robotState.distanceTelemetreExtremeGauche < 15) { //
        positionObstacle = OBSTACLE_EN_FACE;
       
    } 
    else if (robotState.distanceTelemetreDroit < 25 &&
            robotState.distanceTelemetreCentre > 20 &&
            robotState.distanceTelemetreGauche < 25 &&           
            robotState.distanceTelemetreExtremeDroite < 20 &&
            robotState.distanceTelemetreExtremeGauche < 20) { // Cas "couloir" ou le robot n'a rien devant lui mais des obstacles partout ailleurs
        positionObstacle = PAS_OBSTACLE_PROCHE;
        
    } 
    else if (robotState.distanceTelemetreCentre > 20 &&
            robotState.distanceTelemetreExtremeDroite < 20 &&    
            robotState.distanceTelemetreExtremeGauche > 20) { //
        compteur_extreme = compteur_extreme + 1;
        positionObstacle = OBSTACLE_EXTREME_DROITE;
     
    } 
    else if (robotState.distanceTelemetreCentre > 20 &&
            robotState.distanceTelemetreExtremeDroite > 20 &&    
            robotState.distanceTelemetreExtremeGauche < 20) { //
        compteur_extreme = compteur_extreme + 1;
        positionObstacle = OBSTACLE_EXTREME_GAUCHE;
        
    } 
    else if (robotState.distanceTelemetreCentre > 20 &&
            robotState.distanceTelemetreDroit > 20 &&             
            robotState.distanceTelemetreGauche < 20) { //
        compteur_extreme = compteur_extreme + 1;
        positionObstacle = OBSTACLE_A_GAUCHE;
      
    } 
    else if (robotState.distanceTelemetreCentre > 20 &&
            robotState.distanceTelemetreDroit < 20 &&  
            robotState.distanceTelemetreGauche > 20) { //
        compteur_extreme = compteur_extreme + 1;
       
        positionObstacle = OBSTACLE_A_DROITE;
    } 
    else if (robotState.distanceTelemetreExtremeDroite < 20 &&
            robotState.distanceTelemetreCentre < 20 &&
            robotState.distanceTelemetreExtremeDroite < robotState.distanceTelemetreExtremeGauche) { //
        positionObstacle = OBSTACLE_DEVANT_DROITE;
        compteur_extreme = 0;
        
    } 
    else if (robotState.distanceTelemetreExtremeGauche < 20 &&
            robotState.distanceTelemetreCentre < 20) { //
        positionObstacle = OBSTACLE_DEVANT_GAUCHE;
        compteur_extreme = 0;
       
    } 
    else if (robotState.distanceTelemetreDroit < 25 &&
            robotState.distanceTelemetreCentre > 20 &&
            robotState.distanceTelemetreGauche >= robotState.distanceTelemetreDroit) { //Obstacle a droite
        positionObstacle = OBSTACLE_A_DROITE;
        compteur_extreme = 0;
        
    } 
    else if (robotState.distanceTelemetreGauche < 25 &&
            robotState.distanceTelemetreCentre > 20) { //Obstacle a gauche
        positionObstacle = OBSTACLE_A_GAUCHE;
        compteur_extreme = 0;
       
    } 
    else if (robotState.distanceTelemetreCentre < 25) { //Obstacle en face
        positionObstacle = OBSTACLE_EN_FACE;
        compteur_extreme = 0;
        
    } 
    else { //pas d'obstacle
        positionObstacle = PAS_OBSTACLE_PROCHE ;
        compteur_extreme = 0 ;                       // Enclenche une vitesse reduite (20) si aucun des cas n'est valide
        
    }


    //D�termination de l?�tat � venir du robot
    if (positionObstacle == PAS_D_OBSTACLE)
        nextStateRobot = STATE_AVANCE;
    else if (positionObstacle == PAS_OBSTACLE_PROCHE)
        nextStateRobot = STATE_RALENTIT;
    else if (positionObstacle == OBSTACLE_A_DROITE)
        nextStateRobot = STATE_TOURNE_GAUCHE;
    else if (positionObstacle == OBSTACLE_A_GAUCHE)
        nextStateRobot = STATE_TOURNE_DROITE;
    else if (positionObstacle == OBSTACLE_EN_FACE) {
        while (compteur_marche_arriere < 600) {
            nextStateRobot = STATE_MARCHE_ARRIERE;
            compteur_marche_arriere = compteur_marche_arriere + 1;
        }
        compteur_marche_arriere = 0;
        nextStateRobot = STATE_TOURNE_SUR_PLACE_GAUCHE;
    } else if (positionObstacle == OBSTACLE_EXTREME_GAUCHE)
        nextStateRobot = STATE_TOURNE_VITE_DROITE;
    else if (positionObstacle == OBSTACLE_EXTREME_DROITE)
        nextStateRobot = STATE_TOURNE_VITE_GAUCHE;
    else if (positionObstacle == OBSTACLE_DEVANT_GAUCHE) {
        if (compteur_extreme == 3000) {
            while (compteur_extreme > 0) {
                nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
                compteur_extreme = compteur_extreme - 1;
            }
        }
        nextStateRobot = STATE_TOURNE_SUR_PLACE_DROITE;
    } else if (positionObstacle == OBSTACLE_DEVANT_DROITE) {
        if (compteur_extreme == 3000) {
            while (compteur_extreme > 0) {
                nextStateRobot = STATE_TOURNE_SUR_PLACE_GAUCHE;
                compteur_extreme = compteur_extreme - 1;
            }
        }
        nextStateRobot = STATE_TOURNE_SUR_PLACE_GAUCHE;
    }
    //Si l?on n?est pas dans la transition de l?�tape en cours
    if (nextStateRobot != stateRobot - 1)
        stateRobot = nextStateRobot;
}