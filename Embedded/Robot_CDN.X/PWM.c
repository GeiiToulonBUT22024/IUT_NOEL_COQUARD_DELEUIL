//Partie PWM
#include <xc.h> // library xc.h inclut tous les uC
#include "IO.h"
#include "PWM.h"
#include "Robot.h"
#include "utilities.h"
#include "UART_Protocol.h"
#include "QEI.h"

#define PWMPER 40.0

void InitPWM(void) {
    PTCON2bits.PCLKDIV = 0b000; //Divide by 1
    PTPER = 100 * PWMPER; //Période en pourcentage

    //Réglage PWM moteur 1 sur hacheur 1
    IOCON1bits.PMOD = 0b11; //PWM I/O pin pair is in the True Independent Output mode
    IOCON1bits.PENL = 1;
    IOCON1bits.PENH = 1;
    FCLCON1 = 0x0003; //éDsactive la gestion des faults

    IOCON2bits.PMOD = 0b11; //PWM I/O pin pair is in the True Independent Output mode
    IOCON2bits.PENL = 1;
    IOCON2bits.PENH = 1;
    FCLCON2 = 0x0003; //éDsactive la gestion des faults

    /* Enable PWM Module */
    PTCONbits.PTEN = 1;
}

/*void PWMSetSpeed(float vitesseEnPourcents, uint8_t moteur) {


    if (moteur == MOTEUR_GAUCHE) {
        robotState.vitesseGaucheCommandeCourante = vitesseEnPourcents;
        if (vitesseEnPourcents > 0) {
            MOTEUR_GAUCHE_L_PWM_ENABLE = 0; //Pilotage de la pin en mode IO
            MOTEUR_GAUCHE_L_IO_OUTPUT = 1; //Mise à 1 de la pin
            MOTEUR_GAUCHE_H_PWM_ENABLE = 1; //Pilotage de la pin en mode PWM
            MOTEUR_GAUCHE_DUTY_CYCLE = Abs(robotState.vitesseGaucheCommandeCourante * PWMPER);
        } else {
            MOTEUR_GAUCHE_H_PWM_ENABLE = 0; //Pilotage de la pin en mode IO
            MOTEUR_GAUCHE_H_IO_OUTPUT = 1; //Mise à 1 de la pin
            MOTEUR_GAUCHE_L_PWM_ENABLE = 1; //Pilotage de la pin en mode PWM
            MOTEUR_GAUCHE_DUTY_CYCLE = Abs(robotState.vitesseGaucheCommandeCourante * PWMPER);
        }
    } else if (moteur == MOTEUR_DROIT) {
        robotState.vitesseDroiteCommandeCourante = vitesseEnPourcents;

        if (vitesseEnPourcents > 0) {
            MOTEUR_DROIT_H_PWM_ENABLE = 0; //Pilotage de la pin en mode IO
            MOTEUR_DROIT_H_IO_OUTPUT = 1; //Mise à 1 de la pin
            MOTEUR_DROIT_L_PWM_ENABLE = 1; //Pilotage de la pin en mode PWM
            MOTEUR_DROIT_DUTY_CYCLE = Abs(robotState.vitesseDroiteCommandeCourante * PWMPER);
        } else {
            MOTEUR_DROIT_L_PWM_ENABLE = 0; //Pilotage de la pin en mode IO
            MOTEUR_DROIT_L_IO_OUTPUT = 1; //Mise à 1 de la pin
            MOTEUR_DROIT_H_PWM_ENABLE = 1; //Pilotage de la pin en mode PWM
            MOTEUR_DROIT_DUTY_CYCLE = Abs(robotState.vitesseDroiteCommandeCourante * PWMPER);
        }

    }
}*/

void PWMSetSpeedConsigne(float consigne, char moteur) {

    //    robotState.consigne = (robotState.vitesseDroiteConsigne + robotState.vitesseGaucheConsigne )/2;
    //    //robotState.consigneAng = (robotState.vitesseDroiteConsigne + robotState.vitesseGaucheConsigne )/DISTROUES;
    //    robotState.consigneAng = 0 ;

    if (moteur == MOTEUR_GAUCHE) robotState.vitesseDroiteConsigne = consigne * COEF_D;
    else if (moteur == MOTEUR_DROIT) robotState.vitesseGaucheConsigne = consigne;
}

double talon = 20;

void PWMUpdateSpeed() {
    // Cette fonction est appelée sur timer et permet de suivre des rampes d?accélération
    if (robotState.vitesseDroiteCommandeCourante < robotState.vitesseDroiteConsigne)
        robotState.vitesseDroiteCommandeCourante = Min(robotState.vitesseDroiteCommandeCourante + robotState.acceleration, robotState.vitesseDroiteConsigne);
    if (robotState.vitesseDroiteCommandeCourante > robotState.vitesseDroiteConsigne)
        robotState.vitesseDroiteCommandeCourante = Max((robotState.vitesseDroiteCommandeCourante - robotState.acceleration), robotState.vitesseDroiteConsigne);

    //    if (robotState.vitesseDroiteCommandeCourante > 0) {
    //        MOTEUR_DROIT_H_PWM_ENABLE = 0; //pilotage de la pin en mode IO
    //        MOTEUR_DROIT_H_IO_OUTPUT = 1; //Mise à 1 de la pin
    //        MOTEUR_DROIT_L_PWM_ENABLE = 1; //Pilotage de la pin en mode PWM
    //    } else {
    //        MOTEUR_DROIT_L_PWM_ENABLE = 0; //pilotage de la pin en mode IO
    //        MOTEUR_DROIT_L_IO_OUTPUT = 1; //Mise à 1 de la pin
    //        MOTEUR_DROIT_H_PWM_ENABLE = 1; //Pilotage de la pin en mode PWM
    //    }
    //    MOTEUR_DROIT_DUTY_CYCLE = Abs(robotState.vitesseDroiteCommandeCourante) * PWMPER;

    if (robotState.vitesseDroiteCommandeCourante >= 0) {
        PDC1 = robotState.vitesseDroiteCommandeCourante * PWMPER + talon;
        SDC1 = talon;
    } else {
        PDC1 = talon;
        SDC1 = -robotState.vitesseDroiteCommandeCourante * PWMPER + talon;
    }


    if (robotState.vitesseGaucheCommandeCourante < robotState.vitesseGaucheConsigne)
        robotState.vitesseGaucheCommandeCourante = Min(robotState.vitesseGaucheCommandeCourante + robotState.acceleration, robotState.vitesseGaucheConsigne);
    if (robotState.vitesseGaucheCommandeCourante > robotState.vitesseGaucheConsigne)
        robotState.vitesseGaucheCommandeCourante = Max(robotState.vitesseGaucheCommandeCourante - robotState.acceleration, robotState.vitesseGaucheConsigne);

    //    if (robotState.vitesseGaucheCommandeCourante > 0) {
    //        MOTEUR_GAUCHE_L_PWM_ENABLE = 0; //pilotage de la pin en mode IO
    //        MOTEUR_GAUCHE_L_IO_OUTPUT = 1; //Mise à 1 de la pin
    //        MOTEUR_GAUCHE_H_PWM_ENABLE = 1; //Pilotage de la pin en mode PWM
    //    } else {
    //        MOTEUR_GAUCHE_H_PWM_ENABLE = 0; //pilotage de la pin en mode IO
    //        MOTEUR_GAUCHE_H_IO_OUTPUT = 1; //Mise à 1 de la pin
    //        MOTEUR_GAUCHE_L_PWM_ENABLE = 1; //Pilotage de la pin en mode PWM
    //    }
    //    MOTEUR_GAUCHE_DUTY_CYCLE = Abs(robotState.vitesseGaucheCommandeCourante) * PWMPER;

    if (robotState.vitesseGaucheCommandeCourante >= 0) {
        PDC2 = robotState.vitesseGaucheCommandeCourante * PWMPER + talon;
        SDC2 = talon;
    } else {
        PDC2 = talon;
        SDC2 = -robotState.vitesseGaucheCommandeCourante * PWMPER + talon;
    }

}

void PWMSetSpeedConsignePolaire(double vitesseLineaire, double vitesseAngulaire) {
    robotState.vitesseDroiteConsigne = COEFF_SPEED_TO_PERCENT * (vitesseLineaire + vitesseAngulaire * DISTROUES / 2);
    robotState.vitesseGaucheConsigne = COEFF_SPEED_TO_PERCENT * (vitesseLineaire - vitesseAngulaire * DISTROUES / 2);
}