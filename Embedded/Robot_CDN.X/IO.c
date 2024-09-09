
#include <xc.h>
#include "IO.h"
//#include "main.h"

void InitIO() {
    // IMPORTANT : d�sactiver les entr�es analogiques, sinon on perd les entr�es num�riques
    ANSELA = 0; // 0 desactive
    ANSELB = 0;
    ANSELC = 0;
    ANSELD = 0;
    ANSELE = 0;
    ANSELG = 0;

    // Configuration des sorties

    //******* LED ***************************
    _TRISJ6 = 0; // LED Blanche 1
    _TRISJ5 = 0; //LED Bleue 1
    _TRISJ4 = 0; // LED Orange 1
    _TRISJ11 = 0; // LED Rouge 1
    _TRISH10 = 0; // LED Verte 1
    _TRISA0 = 0; // LED Blanche 2
    _TRISA9 = 0; //LED Bleue 2
    _TRISK15 = 0; // LED Orange 2
    _TRISA10 = 0; // LED Rouge 2
    _TRISH3 = 0; // LED Verte 2

    //****** Moteurs ************************
    _TRISE3 = 0;
    _TRISE2 = 0;
    _TRISE1 = 0;
    _TRISE0 = 0;


    // Configuration des entr�es

    // Configuration des pins remappables    

    //*************************************************************
    // Unlock Registers
    //*************************************************************
    UnlockIO();

    //Assignation des remappable pins
    _U1RXR = 78; //Remappe la RP... sur l?�entre Rx1 //24
    _RP79R = 0b00001; //Remappe la sortie Tx1 vers RP36R (macro de RPOR1BITS.RP36R)

    //******************** QEI *****************
    _QEA2R = 97; //assign QEI A to pin RP97
    _QEB2R = 113; //assign QEI B to pin RP96
    _QEA1R = 126; //assign QEI A to pin RP70
    _QEB1R = 124; //assign QEI B to pin RP69

    LockIO();
}


//*************************************************************
// Lock Registers
//*************************************************************

void LockIO() {
    asm volatile ("mov #OSCCON,w1 \n"
            "mov #0x46, w2 \n"
            "mov #0x57, w3 \n"
            "mov.b w2,[w1] \n"
            "mov.b w3,[w1] \n"
            "bset OSCCON, #6":: : "w1", "w2", "w3");
}

void UnlockIO() {
    asm volatile ("mov #OSCCON,w1 \n"
            "mov #0x46, w2 \n"
            "mov #0x57, w3 \n"
            "mov.b w2,[w1] \n"
            "mov.b w3,[w1] \n"
            "bclr OSCCON, #6":: : "w1", "w2", "w3");
}


