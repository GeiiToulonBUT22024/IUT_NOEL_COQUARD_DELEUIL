#include <xc.h>
#include "IO.h"
#include "UART_Protocol.h"
#include "CB_TX1.h"
#include "CB_RX1.h"
#include "Robot.h"


unsigned char UartCalculateChecksum(int msgFunction, int msgPayloadLength, unsigned char* msgPayload) {
    //Fonction prenant entree la trame et sa longueur pour calculer le checksum
    unsigned char checksum = 0xFE;

    checksum ^= (unsigned char) (msgFunction >> 8);
    checksum ^= (unsigned char) msgFunction;

    checksum ^= (unsigned char) (msgPayloadLength >> 8);
    checksum ^= (unsigned char) msgPayloadLength;

    for (int i = 0; i < msgPayloadLength; i++) {
        checksum ^= msgPayload[i];
    }

    return checksum;
}

void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, unsigned char* msgPayload) {
    //Fonction d?encodage et d?envoi d?un message
    unsigned char msg[msgPayloadLength + 6];
    int pos = 0;

    msg[pos++] = 0xFE;
    msg[pos++] = (unsigned char) (msgFunction >> 8);
    msg[pos++] = (unsigned char) (msgFunction >> 0);
    msg[pos++] = (unsigned char) (msgPayloadLength >> 8);
    msg[pos++] = (unsigned char) (msgPayloadLength >> 0);

    for (int i = 0; i < msgPayloadLength; i++) {
        msg[pos++] = msgPayload[i];
    }
    msg[pos++] = UartCalculateChecksum(msgFunction, msgPayloadLength, msgPayload);
    SendMessage(msg, pos);
}

int msgDecodedFunction = 0;
int msgDecodedPayloadLength = 0;
unsigned char msgDecodedPayload[128];
int msgDecodedPayloadIndex = 0;
int rcvState = RCV_STATE_WAITING;

void UartDecodeMessage(unsigned char c) {
    //Fonction prenant en entree un octet et servant a reconstituer les trames
    switch (rcvState) {
        case RCV_STATE_WAITING:
            if (c == 0xFE) rcvState = RCV_STATE_FUNCTION_MSB;
            msgDecodedPayloadLength = 0;
            msgDecodedPayloadIndex = 0;
            msgDecodedFunction = 0;
            break;

        case RCV_STATE_FUNCTION_MSB:
            msgDecodedFunction = c << 8;
            rcvState = RCV_STATE_FUNCTION_LSB;
            break;

        case RCV_STATE_FUNCTION_LSB:
            msgDecodedFunction |= c;
            rcvState = RCV_STATE_LENGTH_MSB;
            break;

        case RCV_STATE_LENGTH_MSB:
            msgDecodedPayloadLength = c << 8;
            rcvState = RCV_STATE_LENGTH_LSB;
            break;

        case RCV_STATE_LENGTH_LSB:
            msgDecodedPayloadLength |= c;

            rcvState = RCV_STATE_PAYLOAD;
            break;

        case RCV_STATE_PAYLOAD:
            msgDecodedPayload[msgDecodedPayloadIndex++] = c;
            if (msgDecodedPayloadIndex == msgDecodedPayloadLength)
                rcvState = RCV_STATE_CHECKSUM;
            break;

        case RCV_STATE_CHECKSUM:
            if (UartCalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload) == c) {
                UartProcessDecodedMessage(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
            }
            rcvState = RCV_STATE_WAITING;
            break;

        default:
            rcvState = RCV_STATE_WAITING;
            break;
    }
}

void UartProcessDecodedMessage(int function, int payloadLength, unsigned char* payload) {
    //Fonction appelee apres le decodage pour executer l?action
    //correspondant au message recu
    switch (function) {
        case (int) CMD_ID_LED:
            if (payload[0] == 0) {
                LED_BLANCHE = payload[1];
            } else if (payload[0] == 1) {
                LED_BLEUE = payload[1];
            } else if (payload[0] == 2) {
                LED_ORANGE = payload[1];
            }
            break;
        case (int) CMD_ID_STATE:

            break;

        case (int) CMD_ID_AUTO_MANUAL:
            robotState.autoModeActivated = payload[0];
            break;
        case (int) CMD_ID_TEXT:
        case (int) CMD_ID_TELEMETRE_IR:
        case (int) CMD_ID_CONSIGNE_VITESSE:
            break;

    }
}