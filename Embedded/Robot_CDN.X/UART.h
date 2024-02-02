/* 
 * File:   UART.h
 * Author: TP-EO-1
 *
 * Created on 15 novembre 2023, 09:55
 */

#ifndef UART_H
#define	UART_H
#define BAUDRATE 115200
#define BRGVAL ((FCY/BAUDRATE)/4)-1

void InitUART(void);
void SendMessageDirect(unsigned char* message, int length);


#endif	/* UART_H */

