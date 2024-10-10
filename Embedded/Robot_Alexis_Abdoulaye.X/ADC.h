/* 
 * File:   ADC.h
 * Author: TP-EO-5
 *
 * Created on 27 septembre 2023, 12:14
 */

#ifndef ADC_H
#define	ADC_H

void InitADC1(void);
void ADC1StartConversionSequence();
void ADCClearConversionFinishedFlag(void);
unsigned char ADCIsConversionFinished(void);
unsigned int * ADCGetResult(void);

#endif	/* ADC_H */

