#include <xc.h>
#include <stdio.h>
#include <stdlib.h>
#include "CB_TX1.h"
#define CBTX1_BUFFER_SIZE 128

int cbTx1Head = 0;
int cbTx1Tail = 0;
unsigned char cbTx1Buffer[CBTX1_BUFFER_SIZE];
unsigned char isTransmitting = 0;

void SendMessage(unsigned char *message, int length) {
    unsigned char i = 0;
    if (CB_TX1_GetRemainingSize() >= length) {
        // On peut écrire le message si assez de place
        for (i = 0; i < length; i++)
            CB_TX1_Add(message[i]);
        if (!CB_TX1_IsTranmitting())
            SendOne();
    }
}

void CB_TX1_Add(unsigned char value) {
    // ajouter la valeur au buffer et maj de l'indice
    cbTx1Buffer[cbTx1Head++] = value;
    if (cbTx1Head >= CBTX1_BUFFER_SIZE)
        cbTx1Head = 0;
}

unsigned char CB_TX1_Get(void) {
    // récupère la valeur en tête et déplace la tête au prochain élément
    unsigned char value = cbTx1Buffer[cbTx1Tail++];
    if(cbTx1Tail >= CBTX1_BUFFER_SIZE)
        cbTx1Tail=0;
    return value;
}

void __attribute__((interrupt, no_auto_psv)) _U1TXInterrupt(void) {
    IFS0bits.U1TXIF = 0; // clear TX interrupt flag
    if (cbTx1Tail != cbTx1Head) {
        SendOne();
    } else
        isTransmitting = 0;
}

void SendOne() {
    isTransmitting = 1;
    unsigned char value = CB_TX1_Get();
    U1TXREG = value; // Transmit one character
}

unsigned char CB_TX1_IsTranmitting(void) {
    return isTransmitting;
}

int CB_TX1_GetDataSize(void) {
    if(cbTx1Head>=cbTx1Tail)
        return cbTx1Head-cbTx1Tail;
    else
        return CBTX1_BUFFER_SIZE - (cbTx1Tail-cbTx1Head);
}

int CB_TX1_GetRemainingSize(void) {
    // pour retourner la taille restante du buffer
    return CBTX1_BUFFER_SIZE - CB_TX1_GetDataSize(); 
}
