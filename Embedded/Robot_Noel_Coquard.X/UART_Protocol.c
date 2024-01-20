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
    checksum ^= (unsigned char) (msgFunction >> 8); // Décalage de 8 car un int est codé sur 2 octets 
    checksum ^= (unsigned char) (msgFunction);
    checksum ^= (unsigned char) (msgPayloadLength >> 8); // Décalage de 8 car un int est codé sur 2 octets 
    checksum ^= (unsigned char) (msgPayloadLength);

    for (int i = 0; i < msgPayloadLength; i++) {
        checksum ^= msgPayload[i]; // Fait le ou exclusif sur tous les char du payload
    }

    return checksum;
}

void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, unsigned char* msgPayload) {
    //Fonction d?encodage et d?envoi d?un message
    unsigned char message[msgPayloadLength + 6];
    int pos = 0;
    message[pos++] = 0xFE; // pos ++ pour incrémenter la valeur sans ligne suplémentaire, la variable s'incrémente après avoir assigné la nouvelle valeur
    message[pos++] = (unsigned char) (msgFunction >> 8); // Décalage de 8 car un int est codé sur 2 octets 
    message[pos++] = (unsigned char) msgFunction;
    message[pos++] = (unsigned char) (msgPayloadLength >> 8); // Décalage de 8 car un int est codé sur 2 octets 
    message[pos++] = (unsigned char) msgPayloadLength;

    for (int i = 0; i < msgPayloadLength; i++) {
        message[pos++] = msgPayload[i];
    }

    message[pos] = UartCalculateChecksum(msgFunction, msgPayloadLength, msgPayload); // Appelle la fonction qui calcule le checksum

    SendMessage(message, msgPayloadLength + 6); // Envoie la chaine via la fonction SendMessage fournie
}


//Déclaration des variables utilisées dans les fonctions ci-dessous
int rcvState = ATTENTE;
int msgDecodedFunction = 0;
int msgDecodedPayloadLength = 0;
int msgDecodedPayloadIndex = 0;
unsigned char msgDecodedPayload[128];
int n = 0;
int mode = MANU;
int vit_G;

void UartDecodeMessage(unsigned char c) {
    //Fonction prenant en entree un octet et servant a reconstituer les trames
    switch (rcvState) {
        case ATTENTE:
            if (c == 0xFE) {  //Si le programme recoit le SOF, toutes les variables sont remises à zéro et le programme passe dans l'état suivant
                rcvState = FUNCTION_MSB;
                msgDecodedPayloadLength = 0;
                msgDecodedPayloadIndex = 0;
                msgDecodedFunction = 0;
            }
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
            if (msgDecodedPayloadLength == 0) { //S'il n'y a pas de payload, le checksum est directement calculé
                rcvState = CHECKSUM;
            } else if (msgDecodedPayloadLength > 1024) { // Si les données sont supérieueres à 1024, elles ne sont pas traités puisque ce n'est pas possible
                rcvState = ATTENTE;
            } else {
                rcvState = PAYLOAD; // Sinon on récupère le payload
            }
            break;

        case PAYLOAD:
            msgDecodedPayload[msgDecodedPayloadIndex++] = c;
            if (msgDecodedPayloadIndex >= msgDecodedPayloadLength) {
                rcvState = CHECKSUM;
            }
            break;

        case CHECKSUM:
            if (UartCalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload) == c) { // Si le checsum calcule a la meme valeur que celui reçu, le payload est décodé
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
            LED_ORANGE = (int) payload[0]; // Récupère la valeur du payload, convertit en int et allume la LED en fonction
            break;
        case (int) CODE_LED_BLEUE:
            LED_BLEUE = (int) payload[0];
            break;
        case (int) CODE_LED_BLANCHE:
            LED_BLANCHE = (int) payload[0];
            break;
        case (int) CODE_VITESSE_GAUCHE:
            break;
        case (int) CODE_VITESSE_DROITE:
            break;
        case (int) SET_ROBOT_AUTO:
            mode = AUTO;
            break;
        case (int) SET_ROBOT_MANUAL_CONTROL:
            mode = MANU;
            break;

    }
}

int getMode() { //Fonction utilisée pour récupérer le mode de fonctionnement
    return mode; 
}