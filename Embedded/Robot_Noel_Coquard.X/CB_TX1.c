#include <xc.h>
#include <stdio.h>
#include <stdlib.h>
#include "CB_TX1.h"
#define CBTX1_BUFFER_SIZE 128

int cbTx1Head = 0;
int cbTx1Tail = 0;
unsigned char cbTx1Buffer[CBTX1_BUFFER_SIZE];
unsigned char isTransmitting = 0;

void SendMessage(unsigned char* message, int length) {
    unsigned char i = 0;
    if (CB_TX1_GetRemainingSize() > length) {
        //On peut écrire le message
        for (i = 0; i < length; i++)
            CB_TX1_Add(message[i]);
        if (!CB_TX1_IsTranmitting())
            SendOne();
    }
}

void CB_TX1_Add(unsigned char value) {
    if (CB_TX1_GetRemainingSize() > 0) {
        cbTx1Buffer[cbTx1Tail] = value;
        cbTx1Tail++;
        if (cbTx1Tail == 128) {
            cbTx1Tail = 0;
        }
    }
}

unsigned char CB_TX1_Get(void) {
    char data;
    data = cbTx1Buffer[cbTx1Head];
    cbTx1Head++;
    if (cbTx1Head == 128) {
        cbTx1Head = 0;
    }
    return data;
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
    //return size of data stored in circular buffer
    int dataSize;
    dataSize = 127 - CB_TX1_GetRemainingSize();
    return dataSize;
}

int CB_TX1_GetRemainingSize(void) {
    //return size of remaining size in circular buffer
    int remainingSize;
    if (cbTx1Head > cbTx1Tail) {
        remainingSize = cbTx1Head - cbTx1Tail - 1;
    } 
    else if (cbTx1Head <= cbTx1Tail) {
        remainingSize = CBTX1_BUFFER_SIZE + cbTx1Head - cbTx1Tail - 1;
    }
    return remainingSize;
}
