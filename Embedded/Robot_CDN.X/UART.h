#ifndef UART_H
#define	UART_H

#define BAUDRATE 115200
#define BRGVAL ((FCY/BAUDRATE)/4)-1

void InitUART(void);
void SendMessageDirect(unsigned char* message, int length);

#endif	/* UART_H */