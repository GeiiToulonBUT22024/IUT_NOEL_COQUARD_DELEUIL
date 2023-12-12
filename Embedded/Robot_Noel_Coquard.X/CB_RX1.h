/* 
 * File:   CB_RX1.h
 * Author: Table2
 *
 * Created on 12 décembre 2023, 10:07
 */

#ifndef CB_RX1_H

void CB_RX1_Add(unsigned char value) ;

unsigned char CB_RX1_Get(void) ;

unsigned char CB_RX1_IsDataAvailable(void) ;

void __attribute__((interrupt, no_auto_psv)) _U1RXInterrupt(void) ;

int CB_RX1_GetDataSize(void) ;

int CB_RX1_GetRemainingSize(void) ;

#endif	/* CB_RX1_H */

