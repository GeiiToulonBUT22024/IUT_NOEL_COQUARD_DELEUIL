/* 
 * File:   UART_Protocol.h
 * Author: TP-EO-5
 *
 * Created on 15 janvier 2024, 09:54
 */

#ifndef UART_PROTOCOL_H
#define	UART_PROTOCOL_H

#define STATE_ATTENTE 0
#define STATE_FUNCTION_MSB 1
#define STATE_FUNCTION_LSB 2
#define STATE_PAYLOAD_LENGTH_MSB 3
#define STATE_PAYLOAD_LENGTH_LSB 4
#define STATE_PAYLOAD 5
#define STATE_CHECKSUM 6
#define FUNCTION_TEXT 0x0080
#define FUNCTION_LED1 0x0021
#define FUNCTION_LED2 0x0022
#define FUNCTION_LED3 0x0023
#define FUNCTION_TELEMETRE_GAUCHE 0x0031
#define FUNCTION_TELEMETRE_CENTRE 0x0032
#define FUNCTION_TELEMETRE_DROIT 0x0033
#define FUNCTION_VITESSE_GAUCHE 0x0041
#define FUNCTION_VITESSE_CENTRE 0x0042
#define SET_ROBOT_STATE 0x0051
#define CONFIG_PID 0x0061
#define CONFIG_VLINEAIRE 0x0071
#define CONFIG_VANGULAIRE 0x0072
#define SET_GHOST_POSITION 0x0089


unsigned char UartCalculateChecksum(int msgFunction, int msgPayloadLength, unsigned char* msgPayload);
void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, unsigned char* msgPayload);
void robotStateChange(unsigned char rbState);
void UartProcessDecodedMessage(int function, int payloadLength, unsigned char* payload);
void UartDecodeMessage(unsigned char c);

void UartProcessDecodedMessage_UART2(int rcvFunction, int payloadLength, unsigned char* payload);
void UartDecodeMessage_UART2(unsigned char c);
#endif	/* UART_PROTOCOL_H */

