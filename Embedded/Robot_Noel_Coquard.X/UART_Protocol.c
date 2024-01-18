#include <stdio.h>
#include <stdlib.h>
#include <xc.h>
#include "ChipConfig.h"
#include "UART_Protocol.h"
#include "UART.h"
#include "CB_TX1.h"
#include "CB_RX1.h"
#include "IO.h"
#include "timer.h"
#include "Robot.h"
#include "PWM.h"
#include "main.h"
#include "grafcet.h"

unsigned char UartCalculateChecksum(int msgFunction, int msgPayloadLength, unsigned char* msgPayload) {
    //Fonction prenant entree la trame et sa longueur pour calculer le checksum
    unsigned char checksum = 0;
    checksum ^= 0xFE;
    checksum ^= (unsigned char) (msgFunction >> 8);
    checksum ^= (unsigned char) (msgFunction);
    checksum ^= (unsigned char) (msgPayloadLength >> 8);
    checksum ^= (unsigned char) (msgPayloadLength);

    for (int i = 0; i < msgPayloadLength; i++) {
        checksum ^= msgPayload[i];
    }

    return checksum;
}

void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, unsigned char* msgPayload) {
    //Fonction d?encodage et d?envoi d?un message
    //char b0, b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12 ; 


    unsigned char message[msgPayloadLength + 6];
    int pos = 0;
    message[pos++] = 0xFE;
    message[pos++] = (unsigned char) (msgFunction >> 8);
    message[pos++] = (unsigned char) msgFunction;
    message[pos++] = (unsigned char) (msgPayloadLength >> 8);
    message[pos++] = (unsigned char) msgPayloadLength;

    for (int i = 0; i < msgPayloadLength; i++) {
        message[pos++] = msgPayload[i];
    }

    message[pos] = UartCalculateChecksum(msgFunction, msgPayloadLength, msgPayload);

    /*b0 = message[0] ;
    b1 = message[1] ;
    b2 = message[2] ;
    b3 = message[3] ;
    b4 = message[4] ;
    b5 = message[5] ;
    b6 = message[6] ;
    b7 = message[7] ;
    b8 = message[8] ;
    b9 = message[9] ;
    b10 = message[10] ;
    b11 = message[11] ;
    b12 = message[12] ;*/

    SendMessage(message, msgPayloadLength + 6);
}

int rcvState = ATTENTE;
int msgDecodedFunction = 0;
int msgDecodedPayloadLength = 0;
int msgDecodedPayloadIndex = 0;
unsigned char msgDecodedPayload[128];

void UartDecodeMessage(unsigned char c) {
    //Fonction prenant en entree un octet et servant a reconstituer les trames
    switch (rcvState) {
        case ATTENTE:
            if (c == 0xFE) {
                rcvState = FUNCTION_MSB;
            }
            msgDecodedPayloadLength = 0;
            msgDecodedPayloadIndex = 0;
            msgDecodedFunction = 0;
            break;

        case FUNCTION_MSB:
            msgDecodedFunction = c << 8;
            rcvState = FUNCTION_LSB;
            break;

        case FUNCTION_LSB:
            msgDecodedFunction |= c;
            rcvState = PAYLOAD_LENGTH_MSB;
            break;

        case PAYLOAD_LENGTH_MSB:
            msgDecodedPayloadLength = c << 8;
            rcvState = PAYLOAD_LENGTH_LSB;
            break;

        case PAYLOAD_LENGTH_LSB:
            msgDecodedPayloadLength |= c;
            if (msgDecodedPayloadLength == 0) {
                rcvState = CHECKSUM;
            } else if (msgDecodedPayloadLength > 1024) {
                rcvState = ATTENTE;
            } else {
                rcvState = PAYLOAD;
            }
            break;

        case PAYLOAD:
            msgDecodedPayload[msgDecodedPayloadIndex++] = c;
            if (msgDecodedPayloadIndex >= msgDecodedPayloadLength) {
                rcvState = CHECKSUM;
            }
            break;

        case CHECKSUM:
            if (UartCalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload) == c) {
                UartProcessDecodedMessage(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
            }
            rcvState = ATTENTE;
            break;

        default:
            rcvState = ATTENTE;
            break;
    }
}

void UartProcessDecodedMessage(int function, int payloadLength, unsigned char* payload) {
    //Fonction appelee apres le decodage pour executer l?action
    //correspondant au message recu
    switch (function) {
        case (int) CODE_LED_ORANGE:
            LED_ORANGE = (int) payload;
            break;
        case (int) CODE_LED_BLEUE:
            LED_BLEUE = (int) payload;
            break;
        case (int) CODE_LED_BLANCHE:
            LED_BLANCHE = (int) payload;
            break;
        case (int) CODE_VITESSE_GAUCHE:
            PWMSetSpeedConsigne((int) payload, MOTEUR_GAUCHE);
            break;
        case (int) CODE_VITESSE_DROITE:
            PWMSetSpeedConsigne((int) payload, MOTEUR_DROIT);
            break;
        case (int) SET_ROBOT_AUTO:
            //mode = AUTO ;
            break;
        case (int) SET_ROBOT_MANUAL_CONTROL:
            //mode = MANU ;
            break;

    }
}