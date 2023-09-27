/* 
 * File:   main.c
 * Author: Table2
 *
 * Created on 5 septembre 2023, 12:28
 */

#include <stdio.h>
#include <stdlib.h>
#include <xc.h>
#include "ChipConfig.h"
#include "IO.h"
#include "timer.h"
#include "Robot.h"
#include "PWM.h"

int main(void) {
    /***************************************************************************************************/
    //Initialisation de l?oscillateur
    /****************************************************************************************************/
    InitOscillator();

    /****************************************************************************************************/
    // Configuration des entrées sorties
    /****************************************************************************************************/

    /****************************************************************************************************/
    // Appel des fonctions
    /****************************************************************************************************/
    InitIO();
    InitTimer1();
    InitTimer23();
    InitPWM();

    LED_BLANCHE = 1;
    LED_BLEUE = 1;
    LED_ORANGE = 1;

    PWMSetSpeedConsigne(20, MOTEUR_DROIT);
    PWMSetSpeedConsigne(20, MOTEUR_GAUCHE);

    /****************************************************************************************************/
    // Boucle Principale
    /****************************************************************************************************/
    while (1) {
        //LED_BLANCHE = !LED_BLANCHE;
        //LED_BLEUE = !LED_BLEUE;
        //LED_ORANGE = !LED_ORANGE;
    } // fin main
}