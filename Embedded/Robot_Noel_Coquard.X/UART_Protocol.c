#include <xc.h>
#include "UART_Protocol.h"
#include "UART.h"

#define ATTENTE 0 
#define FUNCTION_MSB 1
#define FUNCTION_LSB 2
#define PAYLOAD_LENGTH_MSB 3
#define PAYLOAD_LENGTH_LSB 4
#define PAYLOAD 5
#define CHECKSUM 6

unsigned char UartCalculateChecksum(int msgFunction, int msgPayloadLength, unsigned char* msgPayload) {
    //Fonction prenant entree la trame et sa longueur pour calculer le checksum
    char checksum = 0;
    checksum ^= 0xFE;
    checksum ^= (char) (msgFunction >> 8);
    checksum ^= (char) (msgFunction);
    checksum ^= (char) (msgPayloadLength >> 8);
    checksum ^= (char) (msgPayloadLength);

    for (int i = 0; i < msgPayloadLength; i++) {
        checksum ^= msgPayload[i];
    }

    return checksum;
}

void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, unsigned char* msgPayload) {
    //Fonction d?encodage et d?envoi d?un message
    //char b0, b1, b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12 ; 


    char message[msgPayloadLength + 6];
    int pos = 0;
    message[pos++] = 0xFE;
    message[pos++] = (char) (msgFunction >> 8);
    message[pos++] = (char) msgFunction;
    message[pos++] = (char) (msgPayloadLength >> 8);
    message[pos++] = (char) msgPayloadLength;

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

void UartDecodeMessage(unsigned char c) {
    //Fonction prenant en entree un octet et servant a reconstituer les trames
    static int rcvState = ATTENTE;
    int msgDecodedFunction = 0;
    int msgDecodedPayloadLength = 0;
    char msgDecodedPayload[];
    int msgDecodedPayloadIndex = 0;

    switch (rcvState) {
        case ATTENTE :
            if (c == 0xFE) {
                rcvState = FUNCTION_MSB;
                msgDecodedPayloadIndex = 0;
            }
            break;

        case FUNCTION_MSB :
            msgDecodedFunction = c << 8;
            rcvState = FUNCTION_LSB;
            break;

        case FUNCTION_LSB :
            msgDecodedFunction |= c;
            rcvState = PAYLOAD_LENGTH_MSB;
            break;

        case PAYLOAD_LENGTH_MSB :
            msgDecodedPayloadLength = c << 8;
            rcvState = PAYLOAD_LENGTH_LSB;
            break;

        case PAYLOAD_LENGTH_LSB :
            msgDecodedPayloadLength |= c;
            char msgDecodedPayload[msgDecodedPayloadLength];
            if (msgDecodedPayloadLength == 0) {
                rcvState = CHECKSUM;
            } else if (msgDecodedPayloadLength > 1024) {
                rcvState = ATTENTE;
            } else {
                rcvState = PAYLOAD;
            }
            break;

        case PAYLOAD :
            msgDecodedPayload[msgDecodedPayloadIndex++] = c;
            if (msgDecodedPayloadIndex >= msgDecodedPayloadLength) {
                rcvState = CHECKSUM;
            }
            break;

        case CHECKSUM :
            char receivedChecksum = c;
            char calculatedChecksum = UartCalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);

            if (calculatedChecksum == receivedChecksum) {
                UartProcessDecodedMessage(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
            } else {
                //textBoxReception.Text += "\nBad CheckSum\n" ;
            }
            rcvState = ATTENTE;
            break;

        default :
            rcvState = ATTENTE;
            break;
    }
}

void UartProcessDecodedMessage(int function, int payloadLength, unsigned char* payload) {
    //Fonction appelee apres le decodage pour executer l?action
    //correspondant au message recu

}