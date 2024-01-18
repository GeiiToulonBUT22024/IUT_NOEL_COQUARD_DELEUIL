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

#define CODE_LED_ORANGE 0x0020
#define CODE_LED_BLEUE 0x0021
#define CODE_LED_BLANCHE 0x0022
#define CODE_TELEMETRE_ED 0x0030
#define CODE_TELEMETRE_D 0x0031
#define CODE_TELEMETRE_C 0x0032
#define CODE_TELEMETRE_G 0x0033
#define CODE_TELEMETRE_EG 0x0034
#define CODE_VITESSE_GAUCHE 0x0040
#define CODE_VITESSE_DROITE 0x0041
#define CODE_TEXT 0x0080

#define SET_ROBOT_AUTO 0x0051
#define SET_ROBOT_MANUAL_CONTROL 0x0052

unsigned char UartCalculateChecksum(int, int, unsigned char*) ;

void UartEncodeAndSendMessage(int, int, unsigned char*) ;

void UartDecodeMessage(unsigned char) ;

void UartProcessDecodedMessage(int, int, unsigned char*) ;

#endif /* UART_H */


