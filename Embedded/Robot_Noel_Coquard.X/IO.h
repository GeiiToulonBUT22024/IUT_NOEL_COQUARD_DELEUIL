#ifndef IO_H
#define IO_H

//Affectation des pins des LEDS
#define LED_ORANGE _LATC10 
#define LED_BLEUE _LATG7
#define LED_BLANCHE _LATG6

//Définitions des pins pour les 6 hacheurs moteurs

#define MOTEUR_GAUCHE_INL MOTEUR1_IN1
#define MOTEUR_GAUCHE_INH MOTEUR1_IN2
#define MOTEUR_GAUCHE_ENH IOCON1bits.PENL
#define MOTEUR_GAUCHE_ENL IOCON1bits.PENH
#define MOTEUR_GAUCHE_DUTY_CYCLE PDC1

// Prototypes fonctions
void InitIO();

#endif /* IO_H */
