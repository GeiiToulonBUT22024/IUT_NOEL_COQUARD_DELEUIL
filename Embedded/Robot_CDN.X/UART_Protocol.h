#ifndef UART_PROTOCOL_H

#define	UART_PROTOCOL_H

#define RCV_STATE_WAITING 0

#define RCV_STATE_FUNCTION_MSB 1
#define RCV_STATE_FUNCTION_LSB 2

#define RCV_STATE_LENGTH_MSB 3
#define RCV_STATE_LENGTH_LSB 4

#define RCV_STATE_PAYLOAD 5
#define RCV_STATE_CHECKSUM 6

unsigned char UartCalculateChecksum(int msgFunction, int msgPayloadLength, unsigned char* msgPayload);
void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, unsigned char* msgPayload);
void UartDecodeMessage(unsigned char c);
void UartProcessDecodedMessage(int function,int payloadLength, unsigned char* payload);

#define CMD_ID_TEXT 0x0080
#define CMD_ID_LED 0x0020
#define CMD_ID_TELEMETRE_IR 0x0030
#define CMD_ID_CONSIGNE_VITESSE 0x0040
#define CMD_ID_STATE 0x0051
#define CMD_ID_AUTO_MANUAL 0x0052
#define CMD_SET_PID 0x0074
#define CMD_SET_CONSIGNE_LIN 0x0075
#define CMD_SET_CONSIGNE_ANG 0x0076
#define CMD_ROBOT_STATE 0x0050
#define CMD_SET_ROBOT_STATE 0x0051
#define CMD_SET_ROBOT_MODE 0x0052
#define CMD_SET_GHOST_POSITION 0x0088

extern int isAsservEnabled;
extern int isGhostEnabled;


#endif	/* UART_PROTOCOL_H */