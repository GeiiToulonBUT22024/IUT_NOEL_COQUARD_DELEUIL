/* 
 * File:   ADC.h
 * Author: TP-EO-1
 *
 * Created on 4 octobre 2023, 09:08
 */

#ifndef ADC_H
#define	ADC_H

void InitADC1(void);
void ADC1StartConversionSequence();
unsigned int * ADCGetResult(void);
unsigned char ADCIsConversionFinished(void);
void ADCClearConversionFinishedFlag(void);


#endif	/* ADC_H */

