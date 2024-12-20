using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using ExtendedSerialPort;
using WpfOscilloscopeControlNew;
using SciChart.Data.Model;
using WpfAsservissementDisplayNew;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using SciChart.Charting.Visuals;
using System.Windows.Controls;

#pragma warning disable CS8618
namespace RobotInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {

        ReliableSerialPort serialPort1;
        DispatcherTimer timerAffichage;
        Robot robot = new Robot();
        public string IRLeft { get; set; }
        public string IRCenter { get; set; }
        public string IRRight { get; set; }
        public string SpeedLeft { get; set; }
        public string SpeedCenter { get; set; }


        public MainWindow()
        {
            // Set this code once in App.xaml.cs or application startup
            SciChartSurface.SetRuntimeLicenseKey("p9wozRRrZOIMawcPuY306xs7a+8VcxP9QlN/zrZBDsBgBECDtg6dJvGZ7Fm/QaC6yB0P1D0Yk4v2VqJk/U5lBuqA1rEhc/kUorxtB7mFAMgV6Z9T2L/StAgNfzMmUTq6NAZYSS8ycAz/Eq78K6jLmDgUywaQTBnLFRsxxvhDYnzkUS9/NbkqB+WlCSfHj6eVp3TmqkUtOmczWpfke5SlpLszKhXvhAG7UuPD9bJlNJuUD9wIBeig/HhCkA1Kptdkr0YF1zHAY1Q9S0RH3Q9nq2PTxPzMv/iiOKpJbYuXwigbKYXD71t/NL0Imx+dfgN50tuX4piPoH2pg+HN2OLnu+9qpzFSdjYMvDi+txocP3unnZhpf1O7JkrjJ5ux+wwTtRXf55S0/QdIBqS6Ko5d5YDYSppXd01m6HqFz6nBnStz2gwnSKoBRfrlX7OdVtA+8PMbLUmoBeYZXtE3MvVciCh7J7cIxiK3x4jAR8yzygtO40ZcSCbATK+uZUFkSOAF00yhzIpR");
            InitializeComponent();
            serialPort1 = new ReliableSerialPort("COM8", 115200, Parity.None, 8, StopBits.One);
            serialPort1.OnDataReceivedEvent += SerialPort1_OnDataReceivedEvent;
            serialPort1.Open();
            timerAffichage = new DispatcherTimer();
            timerAffichage.Interval = new TimeSpan(0, 0, 0, 0, 20);
            timerAffichage.Tick += TimerAffichage_Tick;
            timerAffichage.Start();

            oscilloSpeed.AddOrUpdateLine(0, 200, "Vitesse");
            oscilloSpeed.ChangeLineColor("Vitesse", Colors.Blue);
            oscilloSpeed.isDisplayActivated = true;

            oscilloMutter.AddOrUpdateLine(0, 200, "Position") ;
            oscilloMutter.ChangeLineColor("Position", Colors.Red);
            oscilloMutter.isDisplayActivated= true;

            ghostOscilloPosition.AddOrUpdateLine(0, 800, "Theta Ghost");
            ghostOscilloPosition.ChangeLineColor("Theta Ghost", Colors.Red);
            ghostOscilloPosition.isDisplayActivated = true;

            ghostOscilloSpeedLin.AddOrUpdateLine(0, 600, "Ghost Vitesse Linéaire");
            ghostOscilloSpeedLin.ChangeLineColor("Ghost Vitesse Linéaire", Colors.Orange);
            ghostOscilloSpeedLin.AddOrUpdateLine(1, 600, "Robot Vitesse Linéaire");
            ghostOscilloSpeedLin.ChangeLineColor("Robot Vitesse Linéaire", Colors.Red);
            ghostOscilloSpeedLin.isDisplayActivated = true;

            ghostOscilloSpeed.AddOrUpdateLine(0, 600, "Vitesse Angulaire Ghost");
            ghostOscilloSpeed.AddOrUpdateLine(1, 600, "Vitesse Angulaire Robot");
            ghostOscilloSpeed.ChangeLineColor("Vitesse Angulaire Ghost", Colors.Pink);
            ghostOscilloSpeed.ChangeLineColor("Vitesse Angulaire Robot", Colors.BlueViolet);
            ghostOscilloSpeed.isDisplayActivated = true;
        }

        private void SerialPort1_OnDataReceivedEvent(object ? sender, DataReceivedArgs e)
        {
            robot.receivedText += Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);
            foreach (byte value in e.Data)
            {
                //DecodeMessage(value);
                robot.byteListReceived.Enqueue(value);
            }
        }

        private void TimerAffichage_Tick(object? sender, EventArgs e)
        {
            if (robot.byteListReceived != null)
            {
                while (robot.byteListReceived.Count > 0)
                {
                    DecodeMessage(robot.byteListReceived.Dequeue());
                }

                textBoxReception.Text = "";
                //foreach (string text in robot.TextToPrint)
                //{
                //    textBoxReception.Text += text;
                //}

                foreach (string text in robot.TextDebugAutoMode)
                {
                    textBoxReception.Text += text;
                }
            }

        }

        private void buttonEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }


        private void SendMessage()
        {
            byte[] msgText = Encoding.ASCII.GetBytes(textBoxEmission.Text);
           
            UartEncodeAndSendMessage(0x0052, msgText.Length, msgText);

            textBoxEmission.Clear();
        }

        private void textBoxEmission_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            textBoxReception.Clear();
        }

        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            //byte[] byteList;
            //byteList = new byte[20];
            //for (int i = 0; i<20; i++)
            //{
            //    byteList[i] = (byte)(2*i);
            //}
            //serialPort1.Write(Encoding.UTF8.GetString(byteList, 0, 20));

            byte[] msgText = Encoding.ASCII.GetBytes("Bonjour");
            byte[] msgLed1 = Encoding.ASCII.GetBytes("1");
            byte[] msgTeleL = Encoding.ASCII.GetBytes("40");
            byte[] msgTeleC = Encoding.ASCII.GetBytes("60");
            byte[] msgTeleR = Encoding.ASCII.GetBytes("65");
            byte[] msgVitL = Encoding.ASCII.GetBytes("55");
            byte[] msgVitC = Encoding.ASCII.GetBytes("15");
            UartEncodeAndSendMessage(0x0080, msgText.Length, msgText);
            UartEncodeAndSendMessage(0x0021, msgLed1.Length, msgLed1);
            UartEncodeAndSendMessage(0x0031, msgTeleL.Length, msgTeleL);
            UartEncodeAndSendMessage(0x0032, msgTeleC.Length, msgTeleC);
            UartEncodeAndSendMessage(0x0033, msgTeleR.Length, msgTeleR);
            UartEncodeAndSendMessage(0x0041, msgVitL.Length, msgVitL);
            UartEncodeAndSendMessage(0x0042, msgVitC.Length, msgVitC);
        }

        private byte CalculateChecksum(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            byte checksum = 0;
            checksum ^= 0xFE;
            checksum ^= (byte)(msgFunction >> 8);
            checksum ^= (byte)msgFunction;
            checksum ^= (byte)(msgPayloadLength >> 8);
            checksum ^= (byte)msgPayloadLength;

            foreach (byte b in msgPayload)
            {
                checksum ^= b;
            }

            return checksum;
        }

        void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            byte[] message = new byte[msgPayloadLength + 6];
            int position = 0;
            message[position++] = 0xFE;
            message[position++] = (byte)(msgFunction >> 8);
            message[position++] = (byte)(msgFunction);
            message[position++] = (byte)(msgPayloadLength >> 8);
            message[position++] = (byte)msgPayloadLength;

            foreach (byte b in msgPayload)
            {
                message[position++] = b;
            }

            message[position++] = CalculateChecksum(msgFunction, msgPayloadLength, msgPayload);
            //on avait placé message et dans ce cas là le checksum sera effectué 2 fois en remplaçant quasiment tout sauf le dernier calcul. Celui-ci est ajouté(i.message[position++] = b)

            serialPort1.Write(message, 0, position);
            
        }

        public enum StateReception
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
        int msgDecodedFunction = 0;
        int msgDecodedPayloadLength = 0;
        byte[] msgDecodedPayload;
        int msgDecodedPayloadIndex = 0;
        private void DecodeMessage(byte c)
        {
            switch (rcvState)
            {
                case StateReception.Waiting:
                    if (c == 0xFE)
                        rcvState = StateReception.FunctionMSB;
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
                    if (msgDecodedPayloadLength < 1024)
                    {
                        if (msgDecodedPayloadLength > 0)
                        {
                            rcvState = StateReception.Payload;
                        }
                        else
                        {
                            rcvState = StateReception.CheckSum;
                        }
                    }
                    else
                    {
                        rcvState = StateReception.Waiting;
                    }
                    break;
                case StateReception.Payload:
                    if(msgDecodedPayloadIndex <= msgDecodedPayloadLength)
                    {
                       ( msgDecodedPayload[msgDecodedPayloadIndex]) = c;
                        if(++msgDecodedPayloadIndex >= msgDecodedPayloadLength)
                        {
                            rcvState = StateReception.CheckSum;
                            msgDecodedPayloadIndex = 0;
                        }

                    }
                    break;
                case StateReception.CheckSum:
                    byte calculatedChecksum = c;

                    byte receivedChecksum = CalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
                    if (calculatedChecksum == receivedChecksum)
                    {
                        //Success, on a un message valide
                        ProcessDecodedMessage(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
                    }
                    else
                    {
                        robot.TextToPrint.Add("Les checksums sont différents");
                    }
                    rcvState = StateReception.Waiting;
                    break;
                default:
                    rcvState = StateReception.Waiting;
                    break;
            }
        }

        public enum codeFunction {
            text = 0x0080,
            led1 = 0x0021,
            led2 = 0x0022,
            led3 = 0x0023,
            telemetreL = 0x0031,
            telemetreC = 0x0032,
            telemetreR = 0x0033,
            telemetreEL = 0x0034,
            telemetreER = 0x0035,
            vitesseL = 0x0041,
            vitesseC = 0x0042,
            RobotState = 0x0050,
            positionData = 0x0062,
            confPID = 0x0063,
            vitLinRob = 0x0071,
            vitPolRob = 0x0072,
            codeAsserv = 0x0091,
            ghostData = 0x0010,
        }
        codeFunction rcvFunction;

        public enum StateRobot
        {
            STATE_ATTENTE = 0,
            STATE_ATTENTE_EN_COURS = 1,
            STATE_AVANCE = 2,
            STATE_AVANCE_EN_COURS = 3,
            STATE_TOURNE_GAUCHE = 4,
            STATE_TOURNE_GAUCHE_EN_COURS = 5,
            STATE_TOURNE_DROITE = 6,
            STATE_TOURNE_DROITE_EN_COURS = 7,
            STATE_TOURNE_SUR_PLACE_GAUCHE = 8,
            STATE_TOURNE_SUR_PLACE_GAUCHE_EN_COURS = 9,
            STATE_TOURNE_SUR_PLACE_DROITE = 10,
            STATE_TOURNE_SUR_PLACE_DROITE_EN_COURS = 11,
            STATE_ARRET = 12,
            STATE_ARRET_EN_COURS = 13,
            STATE_RECULE = 14,
            STATE_RECULE_EN_COURS = 15
        }

        void ProcessDecodedMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            rcvFunction = (codeFunction)msgFunction;
            switch (rcvFunction)
            {
                case codeFunction.text:
                    textBoxReception.Text += "Texte reçu : " + Encoding.ASCII.GetString(msgPayload, 0, msgPayloadLength);
                    break;
                case codeFunction.led1:
                    led1.IsChecked = true;
                    break;
                case codeFunction.led2:
                    break;
                case codeFunction.led3:
                    break;
                case codeFunction.telemetreL:
                    //IR_Gauche.Text = "IR Gauche : " + Encoding.ASCII.GetString(msgPayload, 0, msgPayloadLength) + " cm";
                    break;
                case codeFunction.telemetreC:
                   // IR_Centre.Text = "IR Centre : " + Encoding.ASCII.GetString(msgPayload, 0, msgPayloadLength) + " cm";
                    break;
                case codeFunction.telemetreR:
                    //IR_Droit.Text = "IR Droit : " + Encoding.ASCII.GetString(msgPayload, 0, msgPayloadLength) + " cm";
                    break;
                case codeFunction.vitesseL:
                    // Vitesse_Gauche.Text = "Vitesse Gauche : " + Encoding.ASCII.GetString(msgPayload, 0, msgPayloadLength) + " %";
                    break;
                case codeFunction.vitesseC:
                    // Vitesse_Centre.Text = "Vitesse Centre : " + Encoding.ASCII.GetString(msgPayload, 0, msgPayloadLength) + " %";
                    break;
                case codeFunction.RobotState: // old code

                    int timestamp = (((int)msgPayload[1]) << 24) + (((int)msgPayload[2]) << 16) + (((int)msgPayload[3]) << 8) + ((int)msgPayload[4]);
                    string Text_Etat = "\nRobot State: " + ((StateRobot)msgPayload[0]).ToString() + " - " + timestamp.ToString() + " ms";
                    robot.TextDebugAutoMode.Add(Text_Etat);
                    while (robot.TextDebugAutoMode.Count() > 5)
                    {
                        robot.TextDebugAutoMode.RemoveAt(0);
                    }
                    break;


                case codeFunction.positionData:
                    var Tab = msgPayload.Skip(0).Take(4).Reverse().ToArray();
                    robot.timestamp = BitConverter.ToUInt32(Tab, 0);
                    robot.positionXOdo = BitConverter.ToSingle(msgPayload, 4);
                    robot.positionYOdo = BitConverter.ToSingle(msgPayload, 8);
                    robot.angleRadOdo = BitConverter.ToSingle(msgPayload, 12);
                    robot.vitesseLineaire = BitConverter.ToSingle(msgPayload, 16);
                    robot.vitesseAngulaire = BitConverter.ToSingle(msgPayload, 20);

                    // Affichage console
                    //textBoxReception.Text = "PositionX: " + robot.positionXOdo.ToString();
                    //textBoxReception.Text += "\nPositionY: " + robot.positionYOdo.ToString();
                    //textBoxReception.Text += "\n" + robot.timestamp.ToString();

                    // Affichage oscillo 
                    ghostOscilloSpeedLin.AddPointToLine(1, robot.timestamp / 1000.0, robot.vitesseLineaire);

                    ghostOscilloSpeed.AddPointToLine(1, robot.timestamp/1000.0, robot.vitesseAngulaire);
                    break;

                case codeFunction.ghostData:
                    var Tabtimestamp = msgPayload.Skip(0).Take(4).Reverse().ToArray();
                    robot.timestamp = BitConverter.ToUInt32(Tabtimestamp, 0);
                    robot.angleToTargetGhosto = BitConverter.ToSingle(msgPayload, 4);
                    robot.distanceToTargetGhosto = BitConverter.ToSingle(msgPayload, 8);
                    robot.thetaGhosto = BitConverter.ToSingle(msgPayload, 12);
                    robot.vitesseAngGhosto = BitConverter.ToSingle(msgPayload, 16);
                    robot.positionXGhosto = BitConverter.ToSingle(msgPayload, 20);
                    robot.positionYGhosto = BitConverter.ToSingle(msgPayload, 24);
                    robot.vitesseLineaireGhosto = BitConverter.ToSingle(msgPayload, 28);

                    // Affichage oscillo 
                    ghostOscilloPosition.AddPointToLine(0, robot.timestamp / 1000.0, robot.thetaGhosto);
                    ghostOscilloSpeed.AddPointToLine(0, robot.timestamp / 1000.0, robot.vitesseAngGhosto);
                    ghostOscilloSpeedLin.AddPointToLine(0, robot.timestamp / 1000.0, robot.vitesseLineaireGhosto);

                    textBlockTheta.Text = "Theta: " + (robot.thetaGhosto * (180.0f / Math.PI));
                    textBlockAngleToTarget.Text = "Theta Restant : " + (robot.angleToTargetGhosto * (180.0f / Math.PI));
                    textBlockDistanceToTarget.Text = "Distance Restantes : " + robot.distanceToTargetGhosto;
                    textBlockX.Text = "Position x : " + robot.positionXGhosto;
                    textBlockY.Text = "Position Y : " + robot.positionYGhosto;
                    break;

                case codeFunction.confPID:
                    float kpX = BitConverter.ToSingle(msgPayload, 0);
                    float kiX = BitConverter.ToSingle(msgPayload, 4);
                    float kdX = BitConverter.ToSingle(msgPayload, 8);
                    float proportionnelleMaxX = BitConverter.ToSingle(msgPayload, 12);
                    float integralMaxX = BitConverter.ToSingle(msgPayload, 16);
                    float deriveeMaxX = BitConverter.ToSingle(msgPayload, 20);

                    float kpTheta = BitConverter.ToSingle(msgPayload, 24);
                    float kiTheta = BitConverter.ToSingle(msgPayload, 28);
                    float kdTheta = BitConverter.ToSingle(msgPayload, 32);
                    float proportionnelleMaxTheta = BitConverter.ToSingle(msgPayload, 36);
                    float integralMaxTheta = BitConverter.ToSingle(msgPayload, 40);
                    float deriveeMaxTheta = BitConverter.ToSingle(msgPayload, 44);

                    asservSpeedDisplay.UpdatePolarSpeedCorrectionGains(kpX, kpTheta, kiX, kiTheta, kdX, kdTheta);
                    asservSpeedDisplay.UpdatePolarSpeedCorrectionLimits(proportionnelleMaxX, proportionnelleMaxTheta, integralMaxX, integralMaxTheta, deriveeMaxX, deriveeMaxTheta);

                    textBoxReception.Text = "KpX = " + kpX + " kiX = " + kiX + " kdX = " + kdX + "\n" + "PropX = " +
                        proportionnelleMaxX + " IntX = " + integralMaxX + " DerivX = " + deriveeMaxX + "\n\n"

                        + "kpTheta = " + kpTheta + " kiTheta = " + kiTheta + " kdTheta = " + kdTheta + "\n" + "proportionnelleMaxTheta = " +
                        proportionnelleMaxTheta + " integralMaxTheta = " + integralMaxTheta + " deriveeMaxTheta = " + deriveeMaxTheta + "\n";
                    break;

                case codeFunction.codeAsserv:
                    float consigneX = BitConverter.ToSingle(msgPayload, 0);
                    float consigneTheta = BitConverter.ToSingle(msgPayload, 4);
                    float consigneM1 = BitConverter.ToSingle(msgPayload, 8);
                    float consigneM2 = BitConverter.ToSingle(msgPayload, 12);

                    float valueX = BitConverter.ToSingle(msgPayload, 16);
                    float valueTheta = BitConverter.ToSingle(msgPayload, 20);
                    float valueM1 = BitConverter.ToSingle(msgPayload, 24);
                    float valueM2 = BitConverter.ToSingle(msgPayload, 28);

                    float errorX = BitConverter.ToSingle(msgPayload, 32);
                    float errorTheta = BitConverter.ToSingle(msgPayload, 36);
                    float errorM1 = BitConverter.ToSingle(msgPayload, 40);
                    float errorM2 = BitConverter.ToSingle(msgPayload, 44);

                    float commandX = BitConverter.ToSingle(msgPayload, 48);
                    float commandtheta = BitConverter.ToSingle(msgPayload, 52);
                    float commandM1 = BitConverter.ToSingle(msgPayload, 56);
                    float commandM2 = BitConverter.ToSingle(msgPayload, 60);

                    float corrPX = BitConverter.ToSingle(msgPayload, 64);
                    float corrPTheta = BitConverter.ToSingle(msgPayload, 68);

                    float corrIX = BitConverter.ToSingle(msgPayload, 72);
                    float corrITheta = BitConverter.ToSingle(msgPayload, 76);

                    float corrDX = BitConverter.ToSingle(msgPayload, 80);
                    float corrDTheta = BitConverter.ToSingle(msgPayload, 84);

                    asservSpeedDisplay.UpdatePolarSpeedConsigneValues(consigneX, consigneTheta);
                    asservSpeedDisplay.UpdateIndependantSpeedConsigneValues(consigneM1, consigneM2);
                    asservSpeedDisplay.UpdatePolarSpeedCommandValues(commandX, commandtheta);
                    asservSpeedDisplay.UpdateIndependantSpeedCommandValues(commandM1, commandM2);
                    asservSpeedDisplay.UpdatePolarOdometrySpeed(valueX, valueTheta);
                    asservSpeedDisplay.UpdateIndependantOdometrySpeed(valueM1, valueM2);
                    asservSpeedDisplay.UpdatePolarSpeedErrorValues(errorX, errorTheta);
                    asservSpeedDisplay.UpdateIndependantSpeedErrorValues(errorM1, errorM2);
                    asservSpeedDisplay.UpdatePolarSpeedCorrectionValues(corrPX, corrPTheta, corrIX, corrITheta, corrDX, corrDTheta);
                    break;
            }
        }

        #region Micro Funtion
        public long TransformToLong(byte b1, byte b2, byte b3,  byte b4)
        {
            long result = 0;
            result = BitConverter.ToInt32(new byte[] { b1, b2, b3, b4 });
            return result;
        }
        #endregion


        #region Led Event
        private void led1_Checked(object sender, RoutedEventArgs e)
        {
            byte[] message = new byte[1];
            int position = 0;
            message[position++] = 0x01;

            UartEncodeAndSendMessage(0x0021, 1, message);
        }
        private void led1_Unchecked(object sender, RoutedEventArgs e)
        {
            byte[] message = new byte[1];
            int position = 0;
            message[position++] = 0x00;

            UartEncodeAndSendMessage(0x0021, 1, message);
        }
        private void led2_Checked(object sender, RoutedEventArgs e)
        {
            byte[] message = new byte[1];
            int position = 0;
            message[position++] = 0x01;

            UartEncodeAndSendMessage(0x0022, 1, message);
        }

        private void led2_Unchecked(object sender, RoutedEventArgs e)
        {
            byte[] message = new byte[1];
            int position = 0;
            message[position++] = 0x00;

            UartEncodeAndSendMessage(0x0022, 1, message);
        }

        private void led3_Checked(object sender, RoutedEventArgs e)
        {
            byte[] message = new byte[1];
            int position = 0;
            message[position++] = 0x01;

            UartEncodeAndSendMessage(0x0023, 1, message);
        }

        private void led3_Unchecked(object sender, RoutedEventArgs e)
        {
            byte[] message = new byte[1];
            int position = 0;
            message[position++] = 0x00;

            UartEncodeAndSendMessage(0x0023, 1, message);
        }
        #endregion

        private void buttonConfigPID_Click(object sender, RoutedEventArgs e)
        {
            float kpX = 1.0f;
            float kiX = 30.0f;
            float kdX = 0.0f;
            float proportionelleMaxX = 100.0f;
            float integralMaxX = 100.0f;
            float deriveeMaxX = 100.0f;

            float kpTheta = 1.0f;
            float kiTheta = 30.0f;
            float kdTheta = 0.0f;
            float proportionelleMaxTheta = 100.0f;
            float integralMaxTheta = 100.0f;
            float deriveeMaxTheta = 100.0f;

            byte[] kpXByte = BitConverter.GetBytes(kpX); //renvoie 4 octets !
            byte[] kiXByte = BitConverter.GetBytes(kiX);
            byte[] kdXByte = BitConverter.GetBytes(kdX);
            byte[] propXByte = BitConverter.GetBytes(proportionelleMaxX);
            byte[] integralXByte = BitConverter.GetBytes(integralMaxX);
            byte[] derivXByte = BitConverter.GetBytes(deriveeMaxX);

            byte[] kpThetaByte = BitConverter.GetBytes(kpTheta);
            byte[] kiThetaByte = BitConverter.GetBytes(kiTheta);
            byte[] kdThetaByte = BitConverter.GetBytes(kdTheta);
            byte[] propThetaByte = BitConverter.GetBytes(proportionelleMaxTheta);
            byte[] integralThetaByte = BitConverter.GetBytes(integralMaxTheta);
            byte[] derivThetaByte = BitConverter.GetBytes(deriveeMaxTheta);


            byte[] message = new byte[48];
            kpXByte.CopyTo(message, 0);
            kiXByte.CopyTo(message, 4);
            kdXByte.CopyTo(message, 8);
            propXByte.CopyTo(message, 12);
            integralXByte.CopyTo(message, 16);
            derivXByte.CopyTo(message, 20);

            kpThetaByte.CopyTo(message, 24);
            kiThetaByte.CopyTo(message, 28);
            kdThetaByte.CopyTo(message, 32);
            propThetaByte.CopyTo(message, 36);
            integralThetaByte.CopyTo(message, 40);
            derivThetaByte.CopyTo(message, 44);

            UartEncodeAndSendMessage(0x0061, message.Length, message);


        }

        private void radioButtonAffranchi_Checked(object sender, RoutedEventArgs e)
        {
            bool mode = true;
            byte[] modeByte = BitConverter.GetBytes(mode);
            byte[] message = new byte[4];
            modeByte.CopyTo(message, 0);
            UartEncodeAndSendMessage(0x0051, message.Length, message);
        }

        private void radioButtonAsservi_Checked(object sender, RoutedEventArgs e)
        {
            bool mode = false;
            byte[] modeByte = BitConverter.GetBytes(mode);
            byte[] message = new byte[4];
            modeByte.CopyTo(message, 0);
            UartEncodeAndSendMessage(0x0051, message.Length, message);
        }

        private void vitesseLinClicked(object sender, RoutedEventArgs e)
        {
            float vitesseLineaire;
            bool success = float.TryParse(textBoxViutessLineaire.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out vitesseLineaire);
            if (success)
            {
                byte[] vitesseLineaireByte = BitConverter.GetBytes(vitesseLineaire);
                byte[] message = new byte[4];
                vitesseLineaireByte.CopyTo(message, 0);
                UartEncodeAndSendMessage(0x0071, message.Length, message);
            }
        }

        private void vitesseAngClicked(object sender, RoutedEventArgs e)
        {
            float vitesseAngulaire;
            bool success = float.TryParse(textBoxViutessAngulaire.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out vitesseAngulaire);
            if (success)
            {
                byte[] vitesseAngulaireByte = BitConverter.GetBytes(vitesseAngulaire);
                byte[] message = new byte[4];
                vitesseAngulaireByte.CopyTo(message, 0);
                UartEncodeAndSendMessage(0x0072, message.Length, message);
            }
        }

        private void coordGhostClicked(object sender, RoutedEventArgs e)
        {
            float ghostX, ghostY;
            bool success = false;

            // Get press Tag
            if (sender is Button eBtn) 
            {
                
                
                if (eBtn.Tag != null)
                {
                    
                    switch (((String)eBtn.Tag))
                    {
                        case "(0;0)":
                            ghostX = 0;
                            ghostY = 0;
                            break;

                        case "(0;1)":
                            ghostX = 0;
                            ghostY = 1;
                            break;

                        case "(1;0)":
                            ghostX = 1;
                            ghostY = 0;
                            break;

                        case "(1;1)":
                            ghostX = 1;
                            ghostY = 1;
                            break;

                        default:
                            ghostX = 0;
                            ghostY = 0;
                            break;
                    }
                }
                else
                {
                    success = float.TryParse(textBoxGhistX.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out ghostX);
                    success &= float.TryParse(textBoxGhistY.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out ghostY);
                }

                if (success || eBtn.Tag != null)
                {
                    byte[] ghostByteX = BitConverter.GetBytes(ghostX);
                    byte[] ghostByteY = BitConverter.GetBytes(ghostY);

                    byte[] message = new byte[8];
                    ghostByteX.CopyTo(message, 0);
                    ghostByteY.CopyTo(message, 4);
                    UartEncodeAndSendMessage(0x0089, message.Length, message);
                }


            }

            
        }
    }
}
