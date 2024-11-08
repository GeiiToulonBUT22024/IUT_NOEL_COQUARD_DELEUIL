#include <xc.h> // library xc.h inclut tous les uC
#include "IO.h"
#include "PWM.h"
#include "Robot.h"
#include "ToolBox.h"
#include "main.h"
#include "QEI.h"

#define PWMPER 24.0 //40 avant 
unsigned char acceleration = 5;

//void InitPWM(void) {
//    PTCON2bits.PCLKDIV = 0b000; //Divide by 1
//    PTPER = 100 * PWMPER; //ÈPriode en pourcentage
//    //ÈRglage PWM moteur 1 sur hacheur 1
//    IOCON1bits.POLH = 1; //High = 1 and active on low =0
//    IOCON1bits.POLL = 1; //High = 1
//    IOCON1bits.PMOD = 0b01; //Set PWM Mode to Redundant
//    FCLCON1 = 0x0003; //ÈDsactive la gestion des faults
//    //Reglage PWM moteur 2 sur hacheur 6
//    IOCON6bits.POLH = 1; //High = 1
//    IOCON6bits.POLL = 1; //High = 1
//    IOCON6bits.PMOD = 0b01; //Set PWM Mode to Redundant
//    FCLCON6 = 0x0003; //ÈDsactive la gestion des faults
//    /* Enable PWM Module */
//    PTCONbits.PTEN = 1;
//}

//=================================MODIF===============================================

void InitPWM(void) {
    PTCON2bits.PCLKDIV = 0b000; //Divide by 1
    PTPER = 100 * PWMPER; //ÈPriode en pourcentage
    //ÈRglage PWM moteur 1 sur hacheur 1
    IOCON1bits.PMOD = 0b11; //PWM I/O pin pair is in the True Independent Output mode
    IOCON1bits.PENL = 1;
    IOCON1bits.PENH = 1;
    FCLCON1 = 0x0003; //ÈDsactive la gestion des faults
    IOCON2bits.PMOD = 0b11; //PWM I/O pin pair is in the True Independent Output mode
    IOCON2bits.PENL = 1;
    IOCON2bits.PENH = 1;
    FCLCON2 = 0x0003; //ÈDsactive la gestion des faults
    /* Enable PWM Module */
    PTCONbits.PTEN = 1;
}
double talon = 20;

void PWMSetSpeed(float vitesseEnPourcents) {
    PDC2 = vitesseEnPourcents * PWMPER + talon;
    SDC2 = talon;
}

//=================================MODIF===============================================

void PWMSetSpeedConsigne(float vitesseEnPourcents, char moteur) {
    if (!moteur) {
        robotState.vitesseGaucheConsigne = vitesseEnPourcents;
    } else if (moteur) {
        robotState.vitesseDroiteConsigne = vitesseEnPourcents;
    }
}

void PWMUpdateSpeed() {
    // Cette fonction est appelee sur timer et permet de suivre des rampes d acceleration
    if (robotState.vitesseDroiteCommandeCourante < robotState.vitesseDroiteConsigne)
        robotState.vitesseDroiteCommandeCourante = Min(
            robotState.vitesseDroiteCommandeCourante + acceleration,
            robotState.vitesseDroiteConsigne);
    if (robotState.vitesseDroiteCommandeCourante > robotState.vitesseDroiteConsigne)
        robotState.vitesseDroiteCommandeCourante = Max(
            robotState.vitesseDroiteCommandeCourante - acceleration,
            robotState.vitesseDroiteConsigne);
    if (robotState.vitesseDroiteCommandeCourante >= 0) {
        PDC2 = robotState.vitesseDroiteCommandeCourante * PWMPER + talon;
        SDC2 = talon;
    } else {
        PDC2 = talon;
        SDC2 = -robotState.vitesseDroiteCommandeCourante * PWMPER + talon;
    }
    if (robotState.vitesseGaucheCommandeCourante < robotState.vitesseGaucheConsigne)
        robotState.vitesseGaucheCommandeCourante = Min(
            robotState.vitesseGaucheCommandeCourante + acceleration,
            robotState.vitesseGaucheConsigne);
    if (robotState.vitesseGaucheCommandeCourante > robotState.vitesseGaucheConsigne)
        robotState.vitesseGaucheCommandeCourante = Max(
            robotState.vitesseGaucheCommandeCourante - acceleration,
            robotState.vitesseGaucheConsigne);
    if (robotState.vitesseGaucheCommandeCourante > 0) {
        PDC1 = robotState.vitesseGaucheCommandeCourante * PWMPER + talon;
        SDC1 = talon;
    } else {
        PDC1 = talon;
        SDC1 = -robotState.vitesseGaucheCommandeCourante * PWMPER + talon;
    }
}

void PWMSetSpeedConsignePolaire(double vitesseLineaire, double vitesseAngulaire) {
    robotState.consigneVitesseLineaire = vitesseLineaire;
    robotState.consigneVitesseAngulaire = vitesseAngulaire;
}

