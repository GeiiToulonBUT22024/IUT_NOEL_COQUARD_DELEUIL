using Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace robotInterface
{

    internal abstract class SerialCommand
    {
        // -----------------------------------
        public enum CommandType
        {
            TEXT = 0x0080,
            CONSIGNE_VITESSE = 0x0040,
            TELEMETRE_IR = 0x0030,
            LED = 0x0020,
            ODOMETRIE = 0x0061
        }
        // -----------------------------------

        public abstract void Process(Robot robot);
        public abstract byte[] MakePayload();

        protected CommandType type;
        protected byte[]? payload;

        public new CommandType GetType()
        {
            return type;
        }

        // -----------------------------------
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
            if (this.payload is null)
            {
                this.payload = Encoding.UTF8.GetBytes(this.text);
            }
            return this.payload;
        }
    }
    // ---------------------------------------------------------
    internal class SerialCommandConsigneVitesse : SerialCommand
    {
        private int vitesseGauche;
        private int vitesseDroite;


        /// A vérifier avec guigui
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

    /*----------------------*/

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

            this.timestamp = BitConverter.ToSingle(payload, 0);
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



}
