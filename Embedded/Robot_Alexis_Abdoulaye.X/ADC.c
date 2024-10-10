#include <xc.h>
#include "ADC.h"
#include "main.h"

unsigned char ADCResultIndex = 0;
static unsigned int ADCResult[5];
unsigned char ADCConversionFinishedFlag;
/****************************************************************************************************/
// Configuration ADC

/****************************************************************************************************/
void InitADC1(void) {
    //cf. ADC Reference Manual page 47
    //Configuration en mode 12 bits mono canal ADC avec conversions successives sur 4 éentres
    /************************************************************/
    //AD1CON1
    /************************************************************/
    AD1CON1bits.ADON = 0; // ADC module OFF - pendant la config
    AD1CON1bits.AD12B = 1; // 0 : 10bits; 1 : 12bits
    AD1CON1bits.FORM = 0b00; // 00 = Integer (DOUT = 0000 dddd dddd dddd)
    AD1CON1bits.ASAM = 0; // 0 = Sampling begins when SAMP bit is set
    AD1CON1bits.SSRC = 0b111; // 111 = Internal counter ends sampling and starts conversion (auto-convert)
    /************************************************************/
    //AD1CON2
    /************************************************************/
    AD1CON2bits.VCFG = 0b000; // 000 : Voltage Reference = AVDD AVss
    AD1CON2bits.CSCNA = 1; // 1 : Enable Channel Scanning
    AD1CON2bits.CHPS = 0b00; // Converts CH0 only
    AD1CON2bits.SMPI = 4; // 4+1 conversions successives avant interrupt
    AD1CON2bits.ALTS = 0;
    AD1CON2bits.BUFM = 0;
    /************************************************************/
    //AD1CON3
    /************************************************************/
    AD1CON3bits.ADRC = 0; // ADC Clock is derived from Systems Clock
    AD1CON3bits.ADCS = 15; // ADC Conversion Clock TAD = TCY * (ADCS + 1)
    AD1CON3bits.SAMC = 15; // Auto Sample Time
    /************************************************************/
    //AD1CON4
    /************************************************************/
    AD1CON4bits.ADDMAEN = 0; // DMA is not used
    /************************************************************/
    //Configuration des ports
    /************************************************************/
    //ADC éutiliss : 0(B0)-8(B8)-9(B9)-10(B10)-11(B11)
    ANSELBbits.ANSB0 = 1;
    ANSELBbits.ANSB8 = 1;
    ANSELBbits.ANSB9 = 1;
    ANSELBbits.ANSB10 = 1;
    ANSELBbits.ANSB11 = 1;
    
    AD1CSSLbits.CSS0 = 1; // Enable AN3 for scan / Extreme gauche
    AD1CSSLbits.CSS8 = 1; // Enable AN6 for scan / Droit
    AD1CSSLbits.CSS9 = 1; // Enable AN11 for scan / Centre
    AD1CSSLbits.CSS10 = 1; // Enable AN15 for scan / Extreme droit
    AD1CSSLbits.CSS11 = 1; // Enable AN16 for scan / Gauche
    
    /* Assign MUXA inputs */
    AD1CHS0bits.CH0SA = 0; // CH0SA bits ignored for CH0 +ve input selection
    AD1CHS0bits.CH0NA = 0; // Select VREF- for CH0 -ve input
    
    IFS0bits.AD1IF = 0; // Clear the A/D interrupt flag bit
    IEC0bits.AD1IE = 1; // Enable A/D interrupt
    AD1CON1bits.ADON = 1; // Turn on the A/D converter
}

/* This is ADC interrupt routine */
void __attribute__((interrupt, no_auto_psv)) _AD1Interrupt(void) {
    IFS0bits.AD1IF = 0;
    ADCResult[0] = ADC1BUF0; // Read the AN3-scan input 1 conversion result
    ADCResult[1] = ADC1BUF1; // Read the AN6 conversion result
    ADCResult[2] = ADC1BUF2; // Read the AN11 conversion result
    ADCResult[4] = ADC1BUF3; // Read the AN15 conversion result
    ADCResult[3] = ADC1BUF4; // Read the AN16 conversion result
  
    ADCConversionFinishedFlag = 1;
}

void ADC1StartConversionSequence() {
    AD1CON1bits.SAMP = 1; //Lance une acquisition ADC
}


unsigned int * ADCGetResult(void) {
    return ADCResult;
}

unsigned char ADCIsConversionFinished(void) {
    return ADCConversionFinishedFlag;
}

void ADCClearConversionFinishedFlag(void) {
    ADCConversionFinishedFlag = 0;
}
