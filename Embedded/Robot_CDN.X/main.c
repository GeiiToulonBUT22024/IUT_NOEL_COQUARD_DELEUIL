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
#include "TrajectoryGenerator.h"


extern unsigned long timestamp;
unsigned char stateRobot;
int isAsservEnabled = 1;
int isGhostEnabled = 0;

static unsigned long lastTime = 0;
static int ledState = 0;

int main(void) {

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
    InitTrajectoryGenerator();

    robotState.acceleration = 2;
    robotState.stop = 0;
    robotState.mode = 1;

    while (1) {

        for (int i = 0; i < CB_RX1_GetDataSize(); i++) {
            unsigned char c = CB_RX1_Get();
            UartDecodeMessage(c);
        }

        if (robotState.stop) {
            // Arreter le robot
            PWMSetSpeedConsigne(STOP, MOTEUR_GAUCHE);
            PWMSetSpeedConsigne(STOP, MOTEUR_DROIT);

        } else {
            if (robotState.mode == 1) // Si mode autonome
            {

                ADCClearConversionFinishedFlag();

                unsigned int *result = ADCGetResult();

                float volts = ((float) result[4]) * 3.3 / 4096 ;
                if (volts < 0.325)volts = 0.325;
                robotState.distanceTelemetreDroit = 34 / volts - 5;
                volts = ((float) result[2]) * 3.3 / 4096 ;
                if (volts < 0.325)volts = 0.325;
                robotState.distanceTelemetreCentre = 34 / volts - 5;
                volts = ((float) result[1]) * 3.3 / 4096 ;
                if (volts < 0.325)volts = 0.325;
                robotState.distanceTelemetreGauche = 34 / volts - 5;
                volts = ((float) result[3]) * 3.3 / 4096 ;
                if (volts < 0.325)volts = 0.325;
                robotState.distanceTelemetreLePen = 34 / volts - 5;
                volts = ((float) result[0]) * 3.3 / 4096 ;
                if (volts < 0.325)volts = 0.325;
                robotState.distanceTelemetreMelanchon = 34 / volts - 5;

                if (robotState.distanceTelemetreMelanchon <= 20) LED_BLANCHE_1 = 1;
                else LED_BLANCHE_1 = 0;
                if (robotState.distanceTelemetreGauche <= 20) LED_BLEUE_1 = 1;
                else LED_BLEUE_1 = 0;
                if (robotState.distanceTelemetreCentre <= 20) LED_ORANGE_1 = 1;
                else LED_ORANGE_1 = 0;
                if (robotState.distanceTelemetreDroit <= 20) LED_ROUGE_1 = 1;
                else LED_ROUGE_1 = 0;
                if (robotState.distanceTelemetreLePen <= 20) LED_VERTE_1 = 1;
                else LED_VERTE_1 = 0;

                // unsigned char tlmMsg[] = {(unsigned char) robotState.distanceTelemetreMelanchon, (unsigned char) robotState.distanceTelemetreGauche, (unsigned char) robotState.distanceTelemetreCentre, (unsigned char) robotState.distanceTelemetreDroit, (unsigned char) robotState.distanceTelemetreLePen};
                // UartEncodeAndSendMessage(CMD_ID_TELEMETRE_IR, 5, tlmMsg);


                /* -------------------- IMPLEMENTATION STRATEGIE --------------------*/
                if (isAsservEnabled) {
                    LED_BLANCHE_2 = 1;
                    LED_BLEUE_2 = 0;
                    LED_ORANGE_2 = 1;

                } else {
                    LED_BLANCHE_2 = 0;
                    LED_BLEUE_2 = 1;
                    LED_ORANGE_2 = 0;

                    float baseGauche = VITESSE;
                    float baseDroite = VITESSE;
                    int isViteVite = 1;

                    if (robotState.distanceTelemetreMelanchon <= 45) {
                        isViteVite = 0;

                        if (robotState.distanceTelemetreMelanchon <= 10) {
                            baseGauche += (-0.4481) * robotState.distanceTelemetreMelanchon + 9.7403;
                            baseDroite -= (-0.4481) * robotState.distanceTelemetreMelanchon + 9.7403;
                        } else {
                            baseGauche += (-0.055) * robotState.distanceTelemetreMelanchon + 5;
                            baseDroite -= (-0.055) * robotState.distanceTelemetreMelanchon + 5;
                        }
                    }

                    if (robotState.distanceTelemetreGauche <= 45) {
                        isViteVite = 0;

                        baseGauche += (-0.075) * robotState.distanceTelemetreGauche + 5;
                        baseDroite -= (-0.075) * robotState.distanceTelemetreGauche + 5;
                    }

                    if (robotState.distanceTelemetreCentre <= 40) {
                        isViteVite = 0;

                        baseGauche -= (-1.25) * robotState.distanceTelemetreCentre + 42.5 + ((robotState.distanceTelemetreMelanchon + robotState.distanceTelemetreGauche) > (robotState.distanceTelemetreLePen + robotState.distanceTelemetreDroit) ? 10 : -10);
                        baseDroite -= (-1.25) * robotState.distanceTelemetreCentre + 42.5 + ((robotState.distanceTelemetreMelanchon + robotState.distanceTelemetreGauche) > (robotState.distanceTelemetreLePen + robotState.distanceTelemetreDroit) ? -10 : 10);
                    }

                    if (robotState.distanceTelemetreDroit <= 45) {
                        isViteVite = 0;

                        baseGauche -= (-0.075) * robotState.distanceTelemetreDroit + 5;
                        baseDroite += (-0.075) * robotState.distanceTelemetreDroit + 5;
                    }

                    if (robotState.distanceTelemetreLePen <= 45) {
                        isViteVite = 0;

                        if (robotState.distanceTelemetreLePen <= 10) {
                            baseGauche -= (-0.4481) * robotState.distanceTelemetreLePen + 9.7403;
                            baseDroite += (-0.4481) * robotState.distanceTelemetreLePen + 9.7403;
                        } else {
                            baseGauche -= (-0.055) * robotState.distanceTelemetreLePen + 5;
                            baseDroite += (-0.055) * robotState.distanceTelemetreLePen + 5;
                        }
                    }
                    if (isViteVite) {
                        PWMSetSpeedConsigne(VITE_VITE, MOTEUR_GAUCHE);
                        PWMSetSpeedConsigne(VITE_VITE, MOTEUR_DROIT);
                        //            LED_ORANGE_1 = 1;
                        //            LED_BLEUE_1 = 1;
                        //            LED_BLANCHE_1 = 1;
                    } else {
                        PWMSetSpeedConsigne(baseGauche, MOTEUR_GAUCHE);
                        PWMSetSpeedConsigne(baseDroite, MOTEUR_DROIT);
                        //            LED_ORANGE_1 = 0;
                        //            LED_BLEUE_1 = 0;
                        //            LED_BLANCHE_1 = 0;
                    }
                }

                // unsigned char consMsg[] = {(char) robotState.vitesseGaucheConsigne, (char) robotState.vitesseDroiteConsigne};
                // UartEncodeAndSendMessage(CMD_ID_CONSIGNE_VITESSE, 2, conMsg);

            } else // Si mode manuel
            {
                switch (stateRobot) {
                    case STATE_ARRET:
                        PWMSetSpeedConsigne(STOP, MOTEUR_GAUCHE);
                        PWMSetSpeedConsigne(STOP, MOTEUR_DROIT);

                        if ((timestamp - lastTime) >= 300) {
                            lastTime = timestamp;

                            switch (ledState) {
                                case 0:
                                    LED_BLANCHE_1 = 1;
                                    LED_BLEUE_1 = 0;
                                    LED_ORANGE_1 = 0;
                                    break;
                                case 1:
                                    LED_BLANCHE_1 = 0;
                                    LED_BLEUE_1 = 1;
                                    LED_ORANGE_1 = 0;
                                    break;
                                case 2:
                                    LED_BLANCHE_1 = 0;
                                    LED_BLEUE_1 = 0;
                                    LED_ORANGE_1 = 1;
                                    break;
                                case 3:
                                    LED_BLANCHE_1 = 0;
                                    LED_BLEUE_1 = 1;
                                    LED_ORANGE_1 = 0;
                                    break;
                                default:
                                    ledState = 0;
                                    break;
                            }

                            ledState = (ledState + 1) % 4;
                        }
                        break;

                    case STATE_AVANCE:
                        PWMSetSpeedConsigne(20, MOTEUR_DROIT);
                        PWMSetSpeedConsigne(20, MOTEUR_GAUCHE);
                        LED_BLANCHE_1 = 1;
                        LED_BLEUE_1 = 1;
                        LED_ORANGE_1 = 1;
                        break;

                    case STATE_RECULE:
                        PWMSetSpeedConsigne(-20, MOTEUR_DROIT);
                        PWMSetSpeedConsigne(-20, MOTEUR_GAUCHE);
                        LED_BLANCHE_1 = 0;
                        LED_BLEUE_1 = 0;
                        LED_ORANGE_1 = 0;
                        break;

                    case STATE_TOURNE_SUR_PLACE_GAUCHE:
                        PWMSetSpeedConsigne(15, MOTEUR_DROIT);
                        PWMSetSpeedConsigne(-15, MOTEUR_GAUCHE);
                        LED_BLANCHE_1 = 1;
                        LED_BLEUE_1 = 0;
                        LED_ORANGE_1 = 0;
                        break;

                    case STATE_TOURNE_SUR_PLACE_DROITE:
                        PWMSetSpeedConsigne(-15, MOTEUR_DROIT);
                        PWMSetSpeedConsigne(15, MOTEUR_GAUCHE);
                        LED_BLANCHE_1 = 0;
                        LED_BLEUE_1 = 0;
                        LED_ORANGE_1 = 1;
                        break;

                    default:
                        stateRobot = STATE_ARRET;
                        break;
                }
            }
        }
    }
}