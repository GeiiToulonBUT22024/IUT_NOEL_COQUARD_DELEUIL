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
            TELEMETRE_IR = 0x0030,
            CONSIGNE_VITESSE = 0x0040
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


        public SerialProtocolManager() {}

        public void setRobot(Robot robot) { this.robot = robot;}

        //public void ProcessDecodedMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        //{
        //    switch (msgFunction)
        //    {
        //        case (int)CommandID.TEXT:
        //            this.robot.stringListReceived.Enqueue(Encoding.Default.GetString(msgPayload));
        //            break;

        //        case (int)CommandID.LED:
        //            if (msgPayload[0] == 0x00)
        //            {
        //                this.robot.ledBlanche = msgPayload[1];
        //            }
        //            else if (msgPayload[0] == 0x01)
        //            {
        //                this.robot.ledBleue = msgPayload[1];
        //            }
        //            else if (msgPayload[0] == 0x10) {
        //                this.robot.ledOrange = msgPayload[1];
        //            }
        //            break;

        //        case (int)CommandID.TELEMETRE_IR:
        //            this.robot.distanceTelemetreMelenchon = msgPayload[0];
        //            this.robot.distanceTelemetreGauche = msgPayload[1];
        //            this.robot.distanceTelemetreCentre = msgPayload[2];
        //            this.robot.distanceTelemetreDroit = msgPayload[3];
        //            this.robot.distanceTelemetreLePen = msgPayload[4];
        //            break;

        //        case (int)CommandID.CONSIGNE_VITESSE:
        //            this.robot.consigneGauche = (msgPayload[0] > 127 ? msgPayload[0]-256 : msgPayload[0]);
        //            this.robot.consigneDroite = (msgPayload[1] > 127 ? msgPayload[1] - 256 : msgPayload[1]);
        //            break;

        //    }
        //}

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
                        // Debug.WriteLine("Success ID : " + msgDecodedFunction.ToString("X2") + " : " + msgDecodedPayload[0].ToString() + msgDecodedPayload[1].ToString());
                        // ProcessDecodedMessage(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
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
            checksum ^= (byte) msgPayloadLength;

            for (int i = 0; i < msgPayloadLength; i++) {
                checksum ^= msgPayload[i];
            }

            return checksum;
        }


        
        public byte[] UartEncode(SerialCommand cmd)
        {
            byte[] msgPayload = cmd.MakePayload();
            int msgFunction = (int) cmd.GetType();
            int msgPayloadLength = msgPayload.Length;

            List<byte> payload = new List<byte>();
            payload.Add((byte)0xFE);
            payload.Add((byte)(msgFunction >> 8));
            payload.Add((byte)msgFunction);

            payload.Add((byte)(msgPayloadLength >> 8));
            payload.Add((byte)msgPayloadLength);

            for(int i = 0; i < msgPayloadLength;i++)
            {
                payload.Add(msgPayload[i]);
            }

            payload.Add(CalculateChecksum(msgFunction, msgPayloadLength, msgPayload));
            return payload.ToArray();
            
        }
    }
}

