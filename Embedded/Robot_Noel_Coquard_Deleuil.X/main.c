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
#include "main.h"
#include "grafcet.h"
#include "UART.h"
#include "ADC.h"
#include "CB_TX1.h"
#include "CB_RX1.h"
#include <libpic30.h>
#include "UART_Protocol.h"

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
    InitTimer4();
    InitTimer23();
    InitPWM();
    InitADC1();
    InitUART();
    InitQEI1() ;
    InitQEI2() ;

    LED_BLANCHE = 0;
    LED_BLEUE = 0;
    LED_ORANGE = 0;



    /****************************************************************************************************/
    // Boucle Principale
    /****************************************************************************************************/
    while (1) {
        //if (ADCIsConversionFinished()) {
           
               ADCClearConversionFinishedFlag();
        unsigned int * result = ADCGetResult();

        float volts = ((float) result [1])* 3.3 / 4096 * 3.2;
        robotState.distanceTelemetreDroit = 34 / volts - 5;

        volts = ((float) result [2])* 3.3 / 4096 * 3.2;
        robotState.distanceTelemetreCentre = 34 / volts - 5;

        volts = ((float) result [3])* 3.3 / 4096 * 3.2;
        robotState.distanceTelemetreGauche = 34 / volts - 5;

        volts = ((float) result [4])* 3.3 / 4096 * 3.2;
        robotState.distanceTelemetreExtremeGauche = 34 / volts - 5;

        volts = ((float) result [0])* 3.3 / 4096 * 3.2;
        robotState.distanceTelemetreExtremeDroite = 34 / volts - 5;

        unsigned char tlmMsg[] = {(unsigned char) robotState.distanceTelemetreExtremeGauche, (unsigned char) robotState.distanceTelemetreGauche, (unsigned char) robotState.distanceTelemetreCentre, (unsigned char) robotState.distanceTelemetreDroit, (unsigned char) robotState.distanceTelemetreExtremeDroite};
           UartEncodeAndSendMessage(CMD_ID_TELEMETRE_IR, 5, tlmMsg);
            
            
    }          
}    
            
            