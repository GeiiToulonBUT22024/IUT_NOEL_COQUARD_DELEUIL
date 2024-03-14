using Constants;
using robotInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Printing;
using System.Text;
using System.Threading.Tasks;


namespace robotInterface
{
    internal abstract class SerialCommand
    {
        public enum CommandType
        {
            TEXT = 0x0080,
            CONSIGNE_VITESSE = 0x0040,
            TELEMETRE_IR = 0x0030,
            LED = 0x0020,
            ODOMETRIE = 0x0061,
            ASSERV = 0x0070,
            PID = 0x0072,
            SET_PID = 0x0074,
            SET_CONSIGNE_LIN = 0x0075,
            SET_CONSIGNE_ANG = 0x0076,
            ROBOT_STATE = 0x0050,
            SET_ROBOT_STATE = 0x0051,
            SET_ROBOT_MODE = 0x0052
        }

        public abstract void Process(Robot robot);
        public abstract byte[] MakePayload();

        protected CommandType type;
        protected byte[]? payload;

        public new CommandType GetType()
        {
            return type;
        }

        public static SerialCommand? CreateCommand(int commandCode, byte[] payload)
        {
            switch (commandCode)
            {
                case (int)CommandType.TEXT:
                    return new SerialCommandText(payload);

                case (int)CommandType.LED:
                    return new SerialCommandLED(payload);

                case (int)CommandType.TELEMETRE_IR:
                    return new SerialCommandTelemetreIR(payload);

                case (int)CommandType.CONSIGNE_VITESSE:
                    return new SerialCommandConsigneVitesse(payload);

                case (int)CommandType.ODOMETRIE:
                    return new SerialCommandOdometrie(payload);

                case (int)CommandType.ASSERV:
                    return new SerialCommandAsserv(payload);

                case (int)CommandType.PID:
                    return new SerialCommandPid(payload);

                case (int)CommandType.SET_PID:
                    return new SerialCommandSetPID(payload);

                case (int)CommandType.SET_CONSIGNE_LIN:
                    return new SerialCommandSetPID(payload);

                case (int)CommandType.SET_CONSIGNE_ANG:
                    return new SerialCommandSetPID(payload);

                case (int)CommandType.ROBOT_STATE:
                    return new SerialCommandRobotState(payload);

                case (int)CommandType.SET_ROBOT_STATE:
                    return new SerialCommandSetRobotState(payload);

                case (int)CommandType.SET_ROBOT_MODE:
                    return new SerialCommandSetRobotMode(payload);

                default:
                    break;
            }
            return null;
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandText : SerialCommand
    {
        private string text;

        public SerialCommandText(byte[] payload)
        {
            this.type = CommandType.TEXT;
            this.payload = payload;
            this.text = Encoding.Default.GetString(payload);
        }

        public SerialCommandText(string text)
        {
            this.type = CommandType.TEXT;
            this.text = text;
        }

        public override void Process(Robot robot)
        {
            robot.stringListReceived.Enqueue(text);
        }

        public override byte[] MakePayload()
        {
            if (this.payload == null)
            {
                byte[] textBytes = Encoding.UTF8.GetBytes(this.text);
                this.payload = new byte[textBytes.Length + 1];
                Array.Copy(textBytes, this.payload, textBytes.Length);
            }
            return this.payload;
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandConsigneVitesse : SerialCommand
    {
        private int vitesseGauche;
        private int vitesseDroite;

        public SerialCommandConsigneVitesse(byte[] payload)
        {
            this.type = CommandType.CONSIGNE_VITESSE;
            this.payload = payload;
            this.vitesseGauche = payload[0] > 127 ? payload[0] - 256 : payload[0];
            this.vitesseDroite = payload[1] > 127 ? payload[1] - 256 : payload[1];
        }

        public override void Process(Robot robot)
        {
            robot.consigneGauche = (float)this.vitesseGauche;
            robot.consigneDroite = (float)this.vitesseDroite;
        }

        public override byte[] MakePayload()
        {
            if (this.payload is null)
                throw new NotImplementedException();

            return this.payload;
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandTelemetreIR : SerialCommand
    {
        private int telemetreMelenchon;
        private int telemetreGauche;
        private int telemetreCentre;
        private int telemetreDroit;
        private int telemetreLePen;

        public SerialCommandTelemetreIR(byte[] payload)
        {
            this.type = CommandType.TELEMETRE_IR;
            this.payload = payload;
            this.telemetreMelenchon = payload[0];
            this.telemetreGauche = payload[1];
            this.telemetreCentre = payload[2];
            this.telemetreDroit = payload[3];
            this.telemetreLePen = payload[4];
        }
        public override void Process(Robot robot)
        {
            robot.distanceTelemetreMelenchon = this.telemetreMelenchon;
            robot.distanceTelemetreGauche = this.telemetreGauche;
            robot.distanceTelemetreCentre = this.telemetreCentre;
            robot.distanceTelemetreDroit = this.telemetreDroit;
            robot.distanceTelemetreLePen = this.telemetreLePen;
        }

        public override byte[] MakePayload()
        {
            if (this.payload is null)
                throw new NotImplementedException();

            return this.payload;
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandLED : SerialCommand
    {
        private int numero;
        private byte state;

        public SerialCommandLED(byte[] payload)
        {
            this.type = CommandType.LED;
            this.payload = payload;
            this.numero = payload[0];
            this.state = payload[1];
        }

        public SerialCommandLED(int numero, byte state)
        {
            this.type = CommandType.LED;
            this.numero = numero;
            this.state = state;
        }

        public override void Process(Robot robot)
        {
            switch (this.numero)
            {
                case 0x00:
                    robot.ledBlanche = this.state;
                    break;
                case 0x01:
                    robot.ledBlanche = this.state;
                    break;
                case 0x10:
                    robot.ledBlanche = this.state;
                    break;
            }
        }

        public override byte[] MakePayload()
        {
            if (this.payload is null)
            {
                this.payload = new byte[2];
                this.payload[0] = (byte)this.numero;
                this.payload[1] = (byte)this.state;
            }
            return this.payload;
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandOdometrie : SerialCommand
    {
        private float timestamp;
        private float positionX;
        private float positionY;
        private float angle;
        private float vitLin;
        private float vitAng;

        public SerialCommandOdometrie(byte[] payload)
        {
            this.type = CommandType.ODOMETRIE;
            this.payload = payload;

            for (int i = 0; i < 4; i++)
            {
                this.timestamp += (int)payload[3 - i] << (8 * i);
            }
            this.timestamp = this.timestamp / 1000;
            this.positionX = BitConverter.ToSingle(payload, 4);
            this.positionY = BitConverter.ToSingle(payload, 8);
            this.angle = BitConverter.ToSingle(payload, 12);
            this.vitLin = BitConverter.ToSingle(payload, 16);
            this.vitAng = BitConverter.ToSingle(payload, 20);
        }

        public override void Process(Robot robot)
        {
            robot.timestamp = this.timestamp;
            robot.positionXOdo = this.positionX;
            robot.positionYOdo = this.positionY;
            robot.angle = this.angle;
            robot.vitLin = this.vitLin;
            robot.vitAng = this.vitAng;
        }

        public override byte[] MakePayload()
        {
            if (this.payload is null)
                throw new NotImplementedException();

            return this.payload;
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandAsserv : SerialCommand
    {
        private byte pidChoice;
        private float consigne;
        private float consigneAng;
        private float erreur;
        private float corrP;
        private float corrI;
        private float corrD;
        private float cmdLin;
        private float cmdAng;

        public SerialCommandAsserv(byte[] payload)
        {
            this.type = CommandType.ASSERV;
            this.payload = payload;
            this.pidChoice = payload[0];
            this.consigne = BitConverter.ToSingle(payload, 1);
            this.consigneAng = BitConverter.ToSingle(payload, 5);
            this.erreur = BitConverter.ToSingle(payload, 9);
            this.corrP = BitConverter.ToSingle(payload, 13);
            this.corrI = BitConverter.ToSingle(payload, 17);
            this.corrD = BitConverter.ToSingle(payload, 21);
            this.cmdLin = BitConverter.ToSingle(payload, 25);
            this.cmdAng = BitConverter.ToSingle(payload, 29);
        }

        public override void Process(Robot robot)
        {
            if (pidChoice == Pid.PID_LIN)
            {
                robot.pidLin.consigne = this.consigne;
                robot.pidLin.erreur = this.erreur;
                robot.pidLin.corrP = this.corrP;
                robot.pidLin.corrI = this.corrI;
                robot.pidLin.corrD = this.corrD;
                robot.pidLin.cmdLin = this.cmdLin;
            }
            else if (pidChoice == Pid.PID_ANG)
            {
                robot.pidAng.consigne = this.consigneAng;
                robot.pidAng.erreur = this.erreur;
                robot.pidAng.corrP = this.corrP;
                robot.pidAng.corrI = this.corrI;
                robot.pidAng.corrD = this.corrD;
                robot.pidAng.cmdAng = this.cmdAng;
            }
        }

        public override byte[] MakePayload()
        {
            throw new NotImplementedException();
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandPid : SerialCommand
    {
        private byte pidChoice;
        private float Kp;
        private float Ki;
        private float Kd;
        private float erreurPmax;
        private float erreurImax;
        private float erreurDmax;

        public SerialCommandPid(byte[] payload)
        {
            this.type = CommandType.PID;
            this.payload = payload;
            this.pidChoice = payload[0];
            this.Kp = BitConverter.ToSingle(payload, 1);
            this.Ki = BitConverter.ToSingle(payload, 5);
            this.Kd = BitConverter.ToSingle(payload, 9);
            this.erreurPmax = BitConverter.ToSingle(payload, 13);
            this.erreurImax = BitConverter.ToSingle(payload, 17);
            this.erreurDmax = BitConverter.ToSingle(payload, 21);
        }

        public override void Process(Robot robot)
        {
            if (pidChoice == Pid.PID_LIN)
            {
                robot.pidLin.Kp = this.Kp;
                robot.pidLin.Ki = this.Ki;
                robot.pidLin.Kd = this.Kd;
                robot.pidLin.erreurPmax = this.erreurPmax;
                robot.pidLin.erreurImax = this.erreurImax;
                robot.pidLin.erreurDmax = this.erreurDmax;
            }
            else if (pidChoice == Pid.PID_ANG)
            {
                robot.pidAng.Kp = this.Kp;
                robot.pidAng.Ki = this.Ki;
                robot.pidAng.Kd = this.Kd;
                robot.pidAng.erreurPmax = this.erreurPmax;
                robot.pidAng.erreurImax = this.erreurImax;
                robot.pidAng.erreurDmax = this.erreurDmax;
            }
        }
        public override byte[] MakePayload()
        {
            throw new NotImplementedException();
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandSetPID : SerialCommand
    {
        private byte pidType;
        private float Kp;
        private float Ki;
        private float Kd;
        private float erreurPmax;
        private float erreurImax;
        private float erreurDmax;

        public SerialCommandSetPID(byte pidType, float Kp, float Ki, float Kd, float erreurPmax, float erreurImax, float erreurDmax)
        {
            this.type = CommandType.SET_PID;
            this.pidType = pidType;
            this.Kp = Kp;
            this.Ki = Ki;
            this.Kd = Kd;
            this.erreurPmax = erreurPmax;
            this.erreurImax = erreurImax;
            this.erreurDmax = erreurDmax;
        }

        public SerialCommandSetPID(byte[] payload)
        {
            this.type = CommandType.SET_PID;
            this.pidType = payload[0];
            this.Kp = BitConverter.ToSingle(payload, 1);
            this.Ki = BitConverter.ToSingle(payload, 5);
            this.Kd = BitConverter.ToSingle(payload, 9);
            this.erreurPmax = BitConverter.ToSingle(payload, 13);
            this.erreurImax = BitConverter.ToSingle(payload, 17);
            this.erreurDmax = BitConverter.ToSingle(payload, 21);
        }

        public override void Process(Robot robot)
        {
            if (this.pidType == Pid.PID_LIN)
            {
                robot.pidLin.Kp = this.Kp;
                robot.pidLin.Ki = this.Ki;
                robot.pidLin.Kd = this.Kd;
                robot.pidLin.erreurPmax = this.erreurPmax;
                robot.pidLin.erreurImax = this.erreurImax;
                robot.pidLin.erreurDmax = this.erreurDmax;
            }
            else if (this.pidType == Pid.PID_ANG)
            {
                robot.pidAng.Kp = this.Kp;
                robot.pidAng.Ki = this.Ki;
                robot.pidAng.Kd = this.Kd;
                robot.pidAng.erreurPmax = this.erreurPmax;
                robot.pidAng.erreurImax = this.erreurImax;
                robot.pidAng.erreurDmax = this.erreurDmax;
            }
        }

        public override byte[] MakePayload()
        {
            if (this.payload == null)
            {
                this.payload = new byte[25];
                payload[0] = this.pidType;
                byte[] kpBytes = BitConverter.GetBytes(this.Kp);
                byte[] kiBytes = BitConverter.GetBytes(this.Ki);
                byte[] kdBytes = BitConverter.GetBytes(this.Kd);
                byte[] erreurPmaxBytes = BitConverter.GetBytes(this.erreurPmax);
                byte[] erreurImaxBytes = BitConverter.GetBytes(this.erreurImax);
                byte[] erreurDmaxBytes = BitConverter.GetBytes(this.erreurDmax);

                kpBytes.CopyTo(payload, 1);
                kiBytes.CopyTo(payload, 5);
                kdBytes.CopyTo(payload, 9);
                erreurPmaxBytes.CopyTo(payload, 13);
                erreurImaxBytes.CopyTo(payload, 17);
                erreurDmaxBytes.CopyTo(payload, 21);
            }
            return this.payload;
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandSetconsigneLin : SerialCommand
    {
        private float consigneLin;

        public SerialCommandSetconsigneLin(float consigneLin)
        {
            this.type = CommandType.SET_CONSIGNE_LIN;
            this.consigneLin = consigneLin;
        }

        public SerialCommandSetconsigneLin(byte[] payload)
        {
            this.type = CommandType.SET_CONSIGNE_LIN;
            this.consigneLin = payload[0];
        }

        public override void Process(Robot robot)
        {
            robot.consigneLin = this.consigneLin;
        }

        public override byte[] MakePayload()
        {
            if (this.payload == null)
            {
                this.payload = new byte[5];
                payload[0] = (byte)this.consigneLin;
                byte[] consLinBytes = BitConverter.GetBytes(this.consigneLin);

                consLinBytes.CopyTo(payload, 0);

            }
            return this.payload;
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandSetconsigneAng : SerialCommand
    {
        private float consigneAng;

        public SerialCommandSetconsigneAng(float consigneAng)
        {
            this.type = CommandType.SET_CONSIGNE_ANG;
            this.consigneAng = consigneAng;
        }

        public SerialCommandSetconsigneAng(byte[] payload)
        {
            this.type = CommandType.SET_CONSIGNE_ANG;
            this.consigneAng = payload[0];
        }

        public override void Process(Robot robot)
        {
            robot.consigneAng = this.consigneAng;
        }

        public override byte[] MakePayload()
        {
            if (this.payload == null)
            {
                this.payload = new byte[5];
                payload[0] = (byte)this.consigneAng;
                byte[] consAngBytes = BitConverter.GetBytes(this.consigneAng);

                consAngBytes.CopyTo(payload, 0);
            }
            return this.payload;
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandRobotState : SerialCommand
    {
        private byte state;

        public SerialCommandRobotState(byte state)
        {
            this.type = CommandType.ROBOT_STATE;
            this.state = state;
        }

        public SerialCommandRobotState(byte[] payload)
        {
            this.type = CommandType.ROBOT_STATE;
            this.state = payload[0];
        }

        public override void Process(Robot robot)
        {
            switch (this.state)
            {
                case 0:
                    // afficher state sur receptBox
                    break;

                case 2:
                    break;

                case 8:
                    break;

                case 10:
                    break;

                case 14:
                    break;

                default:
                    break;
            }
        }

        public override byte[] MakePayload()
        {
            if (this.payload is null)
                throw new NotImplementedException();

            return this.payload;
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandSetRobotState : SerialCommand
    {
        private byte state;

        public SerialCommandSetRobotState(byte state)
        {
            this.type = CommandType.SET_ROBOT_STATE;
            this.state = state;
        }

        public SerialCommandSetRobotState(byte[] payload)
        {
            this.type = CommandType.SET_ROBOT_STATE;
            this.state = payload[0];
        }

        public override void Process(Robot robot)
        {

        }

        public override byte[] MakePayload()
        {
            if (this.payload == null)
            {
                this.payload = new byte[5];
                payload[0] = this.state;
                byte[] stateBytes = BitConverter.GetBytes(this.state);

                stateBytes.CopyTo(payload, 1);
            }
            return this.payload;
        }
    }

    // ---------------------------------------------------------
    internal class SerialCommandSetRobotMode : SerialCommand
    {
        private byte mode;

        public SerialCommandSetRobotMode(byte mode)
        {
            this.type = CommandType.SET_ROBOT_MODE;
            this.mode = mode;
        }

        public SerialCommandSetRobotMode(byte[] payload)
        {
            this.type = CommandType.SET_ROBOT_MODE;
            this.mode = payload[0];
        }

        public override void Process(Robot robot)
        {
            robot.mode = this.mode;
        }

        public override byte[] MakePayload()
        {
            if (this.payload == null)
            {
                this.payload = new byte[5];
                payload[0] = this.mode;
                byte[] ModeBytes = BitConverter.GetBytes(this.mode);

                ModeBytes.CopyTo(payload, 1);
            }
            return this.payload;
        }
    }
}