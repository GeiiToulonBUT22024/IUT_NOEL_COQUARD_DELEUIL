/* 
 * File:   CB_RX1.h
 * Author: TP-EO-5
 *
 * Created on 12 décembre 2023, 11:16
 */


#ifndef CB_RX2_H
#define	CB_RX2_H

void CB_RX2_Add(unsigned char value);
unsigned char CB_RX2_Get(void);
unsigned char CB_RX2_IsDataAvailable(void);
int CB_RX2_GetDataSize(void);
int CB_RX2_GetRemainingSize(void);

#endif	/* CB_RX1_H */


