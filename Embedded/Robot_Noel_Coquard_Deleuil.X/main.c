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

    LED_BLANCHE = 1;
    LED_BLEUE = 1;
    LED_ORANGE = 1;

    PWMSetSpeedConsigne(0, MOTEUR_DROIT);
    PWMSetSpeedConsigne(0, MOTEUR_GAUCHE);

    /****************************************************************************************************/
    // Boucle Principale
    /****************************************************************************************************/
    while (1) {
        if (ADCIsConversionFinished()) {
            char message[2];
            //unsigned char * msg = {'\0'} ;
            unsigned int * result = ADCGetResult();
            float volts = ((float) result[0])* 3.3 / 4096 * 3.2;
            robotState.distanceTelemetreExtremeDroite = 34 / volts - 5;
            if (robotState.distanceTelemetreExtremeDroite >100)
                robotState.distanceTelemetreExtremeDroite = 100 ;
            message[1] = (char) (robotState.distanceTelemetreExtremeDroite >> 8);
            message[0] = (char) robotState.distanceTelemetreExtremeDroite;
            UartEncodeAndSendMessage(0x0030, 2, (unsigned char*) message);

            volts = ((float) result[1])* 3.3 / 4096 * 3.2;
            robotState.distanceTelemetreDroit = 34 / volts - 5;
            if (robotState.distanceTelemetreDroit>100)
                robotState.distanceTelemetreDroit = 100 ;
            message[1] = (char) (robotState.distanceTelemetreDroit >> 8);
            message[0] = (char) robotState.distanceTelemetreDroit;
            UartEncodeAndSendMessage(0x0031, 2, (unsigned char*) message);

            volts = ((float) result[2])* 3.3 / 4096 * 3.2;
            robotState.distanceTelemetreCentre = 34 / volts - 5;
            if (robotState.distanceTelemetreCentre>100)
                robotState.distanceTelemetreCentre = 100 ;
            message[1] = (char) (robotState.distanceTelemetreCentre >> 8);
            message[0] = (char) robotState.distanceTelemetreCentre;
            UartEncodeAndSendMessage(0x0032, 2, (unsigned char*) message);

            volts = ((float) result[3])* 3.3 / 4096 * 3.2;
            robotState.distanceTelemetreGauche = 34 / volts - 5;
            if (robotState.distanceTelemetreGauche>100)
                robotState.distanceTelemetreGauche = 100 ;
            message[1] = (char) (robotState.distanceTelemetreGauche >> 8);
            message[0] = (char) robotState.distanceTelemetreGauche;
            UartEncodeAndSendMessage(0x0033, 2, (unsigned char*) message);

            volts = ((float) result[4])* 3.3 / 4096 * 3.2;
            robotState.distanceTelemetreExtremeGauche = 34 / volts - 5;
            if (robotState.distanceTelemetreExtremeGauche>100)
                robotState.distanceTelemetreExtremeGauche = 100 ;
            message[1] = (char) (robotState.distanceTelemetreExtremeGauche >> 8);
            message[0] = (char) robotState.distanceTelemetreExtremeGauche;
            UartEncodeAndSendMessage(0x0034, 2, (unsigned char*) message);

            ADCClearConversionFinishedFlag();
        }
        /*SendMessage((unsigned char*) "Bonjour", 7);
        __delay32(40000000);*/

        /*int i;
        for (i = 0; i < CB_RX1_GetDataSize(); i++) {
            unsigned char c = CB_RX1_Get();
            SendMessage(&c, 1);
        }
        
        unsigned char* payload[7] = {'B', 'o', 'n', 'j', 'o', 'u', 'r'};
        UartEncodeAndSendMessage(0x0080, 7, (unsigned char*)"Bonjour" ) ;
        
        __delay32(40000000) ;*/
    }
}

