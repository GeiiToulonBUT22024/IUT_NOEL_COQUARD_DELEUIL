#include <stdio.h>
#include <math.h>
#include <stdlib.h>
#include <xc.h>
#include "ChipConfig.h"
#include "IO.h"
#include "timer.h"
#include "PWM.h"
#include "Robot.h"
#include "ADC.h"
#include "PWM.h"
#include "main.h"
#include "utilities.h"
#include "UART.h"
#include "CB_TX1.h"
#include "CB_RX1.h"
#include <libpic30.h>
#include "UART_Protocol.h"
#include "QEI.h"
#include "asservissement.h"

extern unsigned long timestamp;
unsigned char stateRobot;
int isAsservEnabled = 0;

void sequenceLEDs(void);

int main(void)
{

    InitOscillator();
    InitIO();
    InitTimer23();
    InitTimer1();
    InitTimer4();
    InitPWM();
    InitADC1();
    InitUART();
    InitQEI1();
    InitQEI2();

    robotState.acceleration = 2;
    robotState.stop = 0;
    robotState.mode = 1;

    while (1)
    {

        for (int i = 0; i < CB_RX1_GetDataSize(); i++)
        {
            unsigned char c = CB_RX1_Get();
            UartDecodeMessage(c);
        }

        if (robotState.stop)
        {
            // Arreter le robot
            PWMSetSpeedConsigne(STOP, MOTEUR_GAUCHE);
            PWMSetSpeedConsigne(STOP, MOTEUR_DROIT);

            sequenceLEDs();

        }
        else
        {

            if (robotState.mode == 1) // Si mode autonome
            {

                ADCClearConversionFinishedFlag();

                unsigned int *result = ADCGetResult();

                float volts = ((float) result[4]) * 3.3 / 4096 * 3.2;
                robotState.distanceTelemetreDroit = 34 / volts - 5;

                volts = ((float) result[2]) * 3.3 / 4096 * 3.2;
                robotState.distanceTelemetreCentre = 34 / volts - 5;

                volts = ((float) result[1]) * 3.3 / 4096 * 3.2;
                robotState.distanceTelemetreGauche = 34 / volts - 5;

                volts = ((float) result[3]) * 3.3 / 4096 * 3.2;
                robotState.distanceTelemetreLePen = 34 / volts - 5;

                volts = ((float) result[0]) * 3.3 / 4096 * 3.2;
                robotState.distanceTelemetreMelanchon = 34 / volts - 5;

                unsigned char tlmMsg[] = {(unsigned char) robotState.distanceTelemetreMelanchon, (unsigned char) robotState.distanceTelemetreGauche, (unsigned char) robotState.distanceTelemetreCentre, (unsigned char) robotState.distanceTelemetreDroit, (unsigned char) robotState.distanceTelemetreLePen};
                // UartEncodeAndSendMessage(CMD_ID_TELEMETRE_IR, 5, tlmMsg);

                /* -------------------- IMPLEMENTATION STRATEGIE --------------------*/
                if (isAsservEnabled)
                {
                    LED_BLANCHE = 1;
                    LED_BLEUE = 0;
                    LED_ORANGE = 1;

                    // logique asserv --> polaire ?
                }
                else
                {
                    LED_BLANCHE = 0;
                    LED_BLEUE = 1;
                    LED_ORANGE = 0;

                    float baseGauche = VITESSE;
                    float baseDroite = VITESSE;
                    int isViteVite = 1;

                    if (robotState.distanceTelemetreMelanchon <= 45)
                    {
                        isViteVite = 0;

                        if (robotState.distanceTelemetreMelanchon <= 10)
                        {
                            baseGauche += (-0.4481) * robotState.distanceTelemetreMelanchon + 9.7403;
                            baseDroite -= (-0.4481) * robotState.distanceTelemetreMelanchon + 9.7403;
                        }
                        else
                        {
                            baseGauche += (-0.055) * robotState.distanceTelemetreMelanchon + 5;
                            baseDroite -= (-0.055) * robotState.distanceTelemetreMelanchon + 5;
                        }
                    }

                    if (robotState.distanceTelemetreGauche <= 45)
                    {
                        isViteVite = 0;

                        baseGauche += (-0.075) * robotState.distanceTelemetreGauche + 5;
                        baseDroite -= (-0.075) * robotState.distanceTelemetreGauche + 5;
                    }

                    if (robotState.distanceTelemetreCentre <= 40)
                    {
                        isViteVite = 0;

                        baseGauche -= (-1.25) * robotState.distanceTelemetreCentre + 42.5 + ((robotState.distanceTelemetreMelanchon + robotState.distanceTelemetreGauche) > (robotState.distanceTelemetreLePen + robotState.distanceTelemetreDroit) ? 10 : -10);
                        baseDroite -= (-1.25) * robotState.distanceTelemetreCentre + 42.5 + ((robotState.distanceTelemetreMelanchon + robotState.distanceTelemetreGauche) > (robotState.distanceTelemetreLePen + robotState.distanceTelemetreDroit) ? -10 : 10);
                    }

                    if (robotState.distanceTelemetreDroit <= 45)
                    {
                        isViteVite = 0;

                        baseGauche -= (-0.075) * robotState.distanceTelemetreDroit + 5;
                        baseDroite += (-0.075) * robotState.distanceTelemetreDroit + 5;
                    }

                    if (robotState.distanceTelemetreLePen <= 45)
                    {
                        isViteVite = 0;

                        if (robotState.distanceTelemetreLePen <= 10)
                        {
                            baseGauche -= (-0.4481) * robotState.distanceTelemetreLePen + 9.7403;
                            baseDroite += (-0.4481) * robotState.distanceTelemetreLePen + 9.7403;
                        }
                        else
                        {
                            baseGauche -= (-0.055) * robotState.distanceTelemetreLePen + 5;
                            baseDroite += (-0.055) * robotState.distanceTelemetreLePen + 5;
                        }
                    }
                    if (isViteVite)
                    {
                        PWMSetSpeedConsigne(VITE_VITE, MOTEUR_GAUCHE);
                        PWMSetSpeedConsigne(VITE_VITE, MOTEUR_DROIT);
                        //            LED_ORANGE = 1;
                        //            LED_BLEUE = 1;
                        //            LED_BLANCHE = 1;
                    }
                    else
                    {
                        PWMSetSpeedConsigne(baseGauche, MOTEUR_GAUCHE);
                        PWMSetSpeedConsigne(baseDroite, MOTEUR_DROIT);
                        //            LED_ORANGE = 0;
                        //            LED_BLEUE = 0;
                        //            LED_BLANCHE = 0;
                    }
                }

                unsigned char conMsg[] = {(char) robotState.vitesseGaucheConsigne, (char) robotState.vitesseDroiteConsigne};
                // UartEncodeAndSendMessage(CMD_ID_CONSIGNE_VITESSE, 2, conMsg);
            }
            else // Si mode manuel
            {
                LED_ORANGE = 1;
                LED_BLEUE = 1;
                LED_BLANCHE = 1;

                switch (stateRobot)
                {
                    case STATE_ATTENTE:
                        PWMSetSpeedConsigne(0, MOTEUR_DROIT);
                        PWMSetSpeedConsigne(0, MOTEUR_GAUCHE);

                    case STATE_AVANCE:
                        PWMSetSpeedConsigne(20, MOTEUR_DROIT);
                        PWMSetSpeedConsigne(20, MOTEUR_GAUCHE);
                        break;

                    case STATE_RECULE:
                        PWMSetSpeedConsigne(-20, MOTEUR_DROIT);
                        PWMSetSpeedConsigne(-20, MOTEUR_GAUCHE);
                        break;

                    case STATE_TOURNE_SUR_PLACE_GAUCHE:
                        PWMSetSpeedConsigne(15, MOTEUR_DROIT);
                        PWMSetSpeedConsigne(-15, MOTEUR_GAUCHE);
                        break;

                    case STATE_TOURNE_SUR_PLACE_DROITE:
                        PWMSetSpeedConsigne(-15, MOTEUR_DROIT);
                        PWMSetSpeedConsigne(15, MOTEUR_GAUCHE);
                        break;

                    default:
                        stateRobot = STATE_ATTENTE;
                        break;
                }
            }
        }
    }
}

void sequenceLEDs() {
    while (1) {
        LED_BLANCHE = 1;
        LED_BLEUE = 0;
        LED_ORANGE = 0;
        __delay_ms(500);

        LED_BLANCHE = 0;
        LED_BLEUE = 1;
        __delay_ms(500);

        LED_BLEUE = 0;
        LED_ORANGE = 1;
        __delay_ms(500);

        LED_ORANGE = 0;
    }
}