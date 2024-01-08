/* 
 * File:   UART_Protocol.h
 * Author: Table2
 *
 * Created on 8 janvier 2024, 08:08
 */

#ifndef UART_H
#define UART_H

unsigned char UartCalculateChecksum(int, int, unsigned char*) ;

void UartEncodeAndSendMessage(int, int, unsigned char*) ;

void UartDecodeMessage(unsigned char) ;

void UartProcessDecodedMessage(int, int, unsigned char*) ;

#endif /* UART_H */


