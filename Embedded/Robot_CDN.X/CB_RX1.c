#include <xc.h>
#include <stdio.h>
#include <stdlib.h>
#include "CB_RX1.h"
#define CBRX1_BUFFER_SIZE 128
int cbRx1Head;
int cbRx1Tail;
unsigned char cbRx1Buffer[CBRX1_BUFFER_SIZE];


void CB_RX1_Add(unsigned char value) {
    if (CB_RX1_GetRemainingSize() > 0) {
        if ((cbRx1Tail + 1) % CBRX1_BUFFER_SIZE != cbRx1Head) {
            // ajouter la valeur au buffer et maj de l'indice
            cbRx1Buffer[cbRx1Tail] = value;
            cbRx1Tail = (cbRx1Tail + 1) % CBRX1_BUFFER_SIZE;
        } else {
            // c'est plein, on fait rien
        }
    }
}

unsigned char CB_RX1_Get(void) {
    // rÃ©cupÃ¨re la valeur en tÃªte et dÃ©place la tÃªte au prochain Ã©lÃ©ment
    unsigned char value = cbRx1Buffer[cbRx1Head];
    cbRx1Head = (cbRx1Head + 1) % CBRX1_BUFFER_SIZE;
    return value;
}

unsigned char CB_RX1_IsDataAvailable(void) {
    if (cbRx1Head != cbRx1Tail)
        return 1;
    else
        return 0;
}

void __attribute__((interrupt, no_auto_psv)) _U1RXInterrupt(void) {
    IFS0bits.U1RXIF = 0; // clear RX interrupt flag
    /* check for receive errors */
    if (U1STAbits.FERR == 1) {
        U1STAbits.FERR = 0;
    }
    /* must clear the overrun error to keep uart receiving */
    if (U1STAbits.OERR == 1) {
        U1STAbits.OERR = 0;
    }
    /* get the data */
    while (U1STAbits.URXDA == 1) {
        CB_RX1_Add(U1RXREG);
    }
}

int CB_RX1_GetDataSize(void) {
    // pour retourner la taille des données stockées (calcul distance tête-queue/ +128 pour valeur tjrs positive)
    int dataSize = (cbRx1Tail - cbRx1Head + CBRX1_BUFFER_SIZE) % CBRX1_BUFFER_SIZE;
    return dataSize;
}

int CB_RX1_GetRemainingSize(void) {
    // pour retourner la taille restante du buffer
    int remainingSize = (CBRX1_BUFFER_SIZE - 1) - CB_RX1_GetDataSize(); // -1 pour différentier la case vide de la case pleine
    return remainingSize;
}