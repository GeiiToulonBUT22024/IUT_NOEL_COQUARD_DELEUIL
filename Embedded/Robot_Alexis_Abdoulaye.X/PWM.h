/* 
 * File:   PWM.h
 * Author: TP-EO-5
 *
 * Created on 13 septembre 2023, 16:47
 */

#ifndef PWM_H
#define	PWM_H
#define MOTEUR_DROIT 0
#define MOTEUR_GAUCHE 1

void InitPWM(void);
//void PWMSetSpeed(float vitesseEnPourcents, unsigned char moteur);
void PWMUpdateSpeed();
void PWMSetSpeedConsigne(float vitesseEnPourcents, char moteur);
void PWMSetSpeedConsignePolaire(double vitesseLineaire, double vitesseAngulaire);

#endif	/* PWM_H */

