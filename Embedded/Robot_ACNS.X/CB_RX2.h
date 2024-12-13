/* 
 * File:   CB_RX2.h
 * Author: E306_PC1
 *
 * Created on 13 décembre 2024, 15:10
 */

#ifndef CB_RX2_H
#define	CB_RX2_H

void CB_RX2_Add(unsigned char value);
unsigned char CB_RX2_Get(void);
unsigned char CB_RX2_IsDataAvailable(void);
int CB_RX2_GetDataSize(void);
int CB_RX2_GetRemainingSize(void);

#endif	/* CB_RX2_H */

