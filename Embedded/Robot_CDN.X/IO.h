#ifndef IO_H
#define IO_H

//Affectation des pins des LEDS
#define LED_ORANGE _LATC10 
#define LED_BLEUE _LATG7
#define LED_BLANCHE _LATG6

//Définitions des pins pour les hacheurs moteurs
#define MOTEUR1_IN1 _LATB14
#define MOTEUR1_IN2 _LATB15
#define MOTEUR6_IN1 _LATC6
#define MOTEUR6_IN2 _LATC7

//Configuration spécifique du moteur gauche
#define MOTEUR_GAUCHE_H_IO_OUTPUT MOTEUR1_IN1
#define MOTEUR_GAUCHE_L_IO_OUTPUT MOTEUR1_IN2
#define MOTEUR_GAUCHE_L_PWM_ENABLE IOCON1bits.PENL
#define MOTEUR_GAUCHE_H_PWM_ENABLE IOCON1bits.PENH
#define MOTEUR_GAUCHE_DUTY_CYCLE PDC1

//Configuration spécifique du moteur droit
#define MOTEUR_DROIT_H_IO_OUTPUT MOTEUR6_IN1
#define MOTEUR_DROIT_L_IO_OUTPUT MOTEUR6_IN2
#define MOTEUR_DROIT_L_PWM_ENABLE IOCON6bits.PENL
#define MOTEUR_DROIT_H_PWM_ENABLE IOCON6bits.PENH
#define MOTEUR_DROIT_DUTY_CYCLE PDC6


// Prototypes fonctions
void InitIO();

#endif /* IO_H */
