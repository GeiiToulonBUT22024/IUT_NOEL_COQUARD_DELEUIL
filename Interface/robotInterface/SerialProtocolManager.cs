using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace robotInterface
{
    internal class SerialProtocolManager
    {
        public enum CommandID
        {
            TEXT = 0x0080,
            LED = 0x0020,
            CONSIGNE_VITESSE = 0x0040,
            TELEMETRE_IR = 0x0030,
            ODOMETRIE = 0x0061,
            ASSERV_DATA = 0x0070,
            PID_DATA = 0x0072,
            SET_PID = 0x0074,
            SET_CONSIGNE_LIN = 0x0075,
            SET_CONSIGNE_ANG = 0x0076,
            ROBOT_STATE = 0x0050,
            SET_ROBOT_STATE = 0x0051,
            SET_ROBOT_MODE = 0x0052,
            SET_GHOST_POSITION = 0x0088,
            GHOST_POSITION = 0x0089,
        }

        private enum StateReception
        {
            Waiting,
            FunctionMSB,
            FunctionLSB,
            PayloadLengthMSB,
            PayloadLengthLSB,
            Payload,
            CheckSum
        }

        StateReception rcvState = StateReception.Waiting;

        private int msgDecodedFunction = 0;
        private int msgDecodedPayloadLength = 0;
        private byte[] msgDecodedPayload;
        private int msgDecodedPayloadIndex = 0;

        private Robot? robot;


#pragma warning disable CS8618
        public SerialProtocolManager() { }
#pragma warning restore CS8618

        public void setRobot(Robot robot) { this.robot = robot; }

        public void DecodeMessage(byte c)
        {
            switch (rcvState)
            {
                case StateReception.Waiting:
                    if (c == 0xFE) rcvState = StateReception.FunctionMSB;
                    msgDecodedPayloadLength = 0;
                    msgDecodedPayloadIndex = 0;
                    msgDecodedFunction = 0;
                    break;

                case StateReception.FunctionMSB:
                    msgDecodedFunction = c << 8;
                    rcvState = StateReception.FunctionLSB;
                    break;

                case StateReception.FunctionLSB:
                    msgDecodedFunction |= c;
                    rcvState = StateReception.PayloadLengthMSB;
                    break;

                case StateReception.PayloadLengthMSB:
                    msgDecodedPayloadLength = c << 8;
                    rcvState = StateReception.PayloadLengthLSB;
                    break;

                case StateReception.PayloadLengthLSB:
                    msgDecodedPayloadLength |= c;

                    msgDecodedPayload = new byte[msgDecodedPayloadLength];
                    rcvState = StateReception.Payload;
                    break;

                case StateReception.Payload:
                    msgDecodedPayload[msgDecodedPayloadIndex++] = c;
                    if (msgDecodedPayloadIndex == msgDecodedPayloadLength)
                        rcvState = StateReception.CheckSum;
                    break;

                case StateReception.CheckSum:

                    if (CalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload) == c)
                    {
                        SerialCommand? cmd = SerialCommand.CreateCommand(msgDecodedFunction, msgDecodedPayload);
                        if ((cmd is not null) && (this.robot is not null))
                            cmd.Process(this.robot);
                    }
                    rcvState = StateReception.Waiting;
                    break;

                default:
                    rcvState = StateReception.Waiting;
                    break;
            }
        }

        public byte CalculateChecksum(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            byte checksum = 0xFE;

            checksum ^= (byte)(msgFunction >> 8);
            checksum ^= (byte)msgFunction;

            checksum ^= (byte)(msgPayloadLength >> 8);
            checksum ^= (byte)msgPayloadLength;

            for (int i = 0; i < msgPayloadLength; i++)
            {
                checksum ^= msgPayload[i];
            }

            return checksum;
        }


        public byte[] UartEncode(SerialCommand cmd)
        {
            byte[] msgPayload = cmd.MakePayload();
            int msgFunction = (int)cmd.GetType();
            int msgPayloadLength = msgPayload.Length;

            List<byte> payload = new List<byte>();
            payload.Add((byte)0xFE);
            payload.Add((byte)(msgFunction >> 8));
            payload.Add((byte)msgFunction);

            payload.Add((byte)(msgPayloadLength >> 8));
            payload.Add((byte)msgPayloadLength);

            for (int i = 0; i < msgPayloadLength; i++)
            {
                payload.Add(msgPayload[i]);
            }

            payload.Add(CalculateChecksum(msgFunction, msgPayloadLength, msgPayload));
            return payload.ToArray();
        }
    }
}