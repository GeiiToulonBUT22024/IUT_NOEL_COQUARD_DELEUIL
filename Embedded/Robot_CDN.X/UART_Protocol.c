#include <xc.h>
#include "IO.h"
#include "UART_Protocol.h"
#include "CB_TX1.h"
#include "CB_RX1.h"
#include "main.h"
#include "Robot.h"
#include "utilities.h"
#include "asservissement.h"
#include "TrajectoryGenerator.h"
#include <string.h> // Pour strcmp

extern unsigned char stateRobot;
extern GhostPosition ghostPosition;

int msgDecodedFunction = 0;
int msgDecodedPayloadLength = 0;
unsigned char msgDecodedPayload[128];
int msgDecodedPayloadIndex = 0;
int rcvState = RCV_STATE_WAITING;

unsigned char UartCalculateChecksum(int msgFunction, int msgPayloadLength, unsigned char* msgPayload)
{
    //Fonction prenant en entree la trame et sa longueur pour calculer le checksum
    unsigned char checksum = 0xFE;

    checksum ^= (unsigned char) (msgFunction >> 8);
    checksum ^= (unsigned char) msgFunction;

    checksum ^= (unsigned char) (msgPayloadLength >> 8);
    checksum ^= (unsigned char) msgPayloadLength;

    for (int i = 0; i < msgPayloadLength; i++)
    {
        checksum ^= msgPayload[i];
    }

    return checksum;
}

void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, unsigned char* msgPayload)
{
    //Fonction d encodage et d envoi d un message
    unsigned char msg[msgPayloadLength + 6];
    int pos = 0;

    msg[pos++] = 0xFE;
    msg[pos++] = (unsigned char) (msgFunction >> 8);
    msg[pos++] = (unsigned char) (msgFunction >> 0);
    msg[pos++] = (unsigned char) (msgPayloadLength >> 8);
    msg[pos++] = (unsigned char) (msgPayloadLength >> 0);

    for (int i = 0; i < msgPayloadLength; i++)
    {
        msg[pos++] = msgPayload[i];
    }
    msg[pos++] = UartCalculateChecksum(msgFunction, msgPayloadLength, msgPayload);
    SendMessage(msg, pos);
}

void UartDecodeMessage(unsigned char c)
{
    //Fonction prenant en entree un octet et servant a reconstituer les trames
    switch (rcvState)
    {
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
            if (UartCalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload) == c)
            {
                UartProcessDecodedMessage(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
            }
            rcvState = RCV_STATE_WAITING;
            break;

        default:
            rcvState = RCV_STATE_WAITING;
            break;
    }
}

void UartProcessDecodedMessage(int function, int payloadLength, unsigned char* payload)
{
    switch (function) // Aiguille la commande recue
    {
        case (int) CMD_ID_LED:
            if (payload[0] == 0)
            {
                LED_BLANCHE = payload[1];
            }
            else if (payload[0] == 1)
            {
                LED_BLEUE = payload[1];
            }
            else if (payload[0] == 2)
            {
                LED_ORANGE = payload[1];
            }
            break;

        case (int) CMD_SET_ROBOT_STATE:
            stateRobot = payload[0];
            break;

        case (int) CMD_SET_ROBOT_MODE:
            robotState.mode = payload[0];
            if (payload[0] == 0x01)
            {
                robotState.mode = MODE_AUTO;
            }
            else if (payload[0] == 0x00)
            {
                robotState.mode = MODE_MANU;
            }
            break;

        case CMD_ID_TEXT:
            payload[payloadLength] = '\0';
            if (strcmp((char*) payload, "asservDisabled") == 0)
            {
                isAsservEnabled = 0;
            }
            else if (strcmp((char*) payload, "asservEnabled") == 0)
            {
                isAsservEnabled = 1;
            }
            else if (strcmp((char*) payload, "STOP") == 0)
            {
                robotState.stop = 1;
            }
            else if (strcmp((char*) payload, "GO") == 0)
            {
                robotState.stop = 0;
            }
            break;

        case (int) CMD_SET_CONSIGNE_LIN:
            memcpy((void*) &robotState.consigneLin, payload, 4);
            break;

        case (int) CMD_SET_CONSIGNE_ANG:
            memcpy((void*) &robotState.consigneAng, payload, 4);
            break;

        case (int) CMD_SET_PID:
            if (payloadLength == 25)
            {
                isAsservEnabled = 1;

                if (payload[0] == 0x00)
                { // PID lineaire

                    memcpy((void*) &robotState.PidLin.Kp, payload + 1, 4);
                    memcpy((void*) &robotState.PidLin.Ki, payload + 5, 4);
                    memcpy((void*) &robotState.PidLin.Kd, payload + 9, 4);
                    memcpy((void*) &robotState.PidLin.erreurPmax, payload + 13, 4);
                    memcpy((void*) &robotState.PidLin.erreurImax, payload + 17, 4);
                    memcpy((void*) &robotState.PidLin.erreurDmax, payload + 21, 4);

                    SetupPidAsservissement(&robotState.PidLin, robotState.PidLin.Kp, robotState.PidLin.Ki, robotState.PidLin.Kd, robotState.PidLin.erreurPmax, robotState.PidLin.erreurImax, robotState.PidLin.erreurDmax);
                }
                else if (payload[0] == 0x01)
                { // PID angulaire

                    memcpy((void*) &robotState.PidAng.Kp, (const void*) (payload + 1), 4);
                    memcpy((void*) &robotState.PidAng.Ki, (const void*) (payload + 5), 4);
                    memcpy((void*) &robotState.PidAng.Kd, (const void*) (payload + 9), 4);
                    memcpy((void*) &robotState.PidAng.erreurPmax, (const void*) (payload + 13), 4);
                    memcpy((void*) &robotState.PidAng.erreurImax, (const void*) (payload + 17), 4);
                    memcpy((void*) &robotState.PidAng.erreurDmax, (const void*) (payload + 21), 4);

                    SetupPidAsservissement(&robotState.PidAng, robotState.PidAng.Kp, robotState.PidAng.Ki, robotState.PidAng.Kd, robotState.PidAng.erreurPmax, robotState.PidAng.erreurImax, robotState.PidAng.erreurDmax);
                }
            }
            break;

        case (int) CMD_SET_GHOST_POSITION:
            isGhostEnabled = 1;
            isAsservEnabled = 1;
            memcpy(&ghostPosition.targetX, payload, 4);
            memcpy(&ghostPosition.targetY, payload + 4, 4);
            break;
    }
}