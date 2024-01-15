/* 
 * File:   UART_Protocol.h
 * Author: Table2
 *
 * Created on 8 janvier 2024, 08:08
 */

#ifndef UART_H
#define UART_H

#define ATTENTE 0 
#define FUNCTION_MSB 1
#define FUNCTION_LSB 2
#define PAYLOAD_LENGTH_MSB 3
#define PAYLOAD_LENGTH_LSB 4
#define PAYLOAD 5
#define CHECKSUM 6

#define CODE_LED 0x0020
#define CODE_TELEMETRE_ED 0x0030
#define CODE_TELEMETRE_D 0x0031
#define CODE_TELEMETRE_C 0x0032
#define CODE_TELEMETRE_G 0x0033
#define CODE_TELEMETRE_EG 0x0034
#define CODE_VITESSE 0x0040

unsigned char UartCalculateChecksum(int, int, unsigned char*) ;

void UartEncodeAndSendMessage(int, int, unsigned char*) ;

void UartDecodeMessage(unsigned char) ;

void UartProcessDecodedMessage(int, int, unsigned char*) ;

#endif /* UART_H */


