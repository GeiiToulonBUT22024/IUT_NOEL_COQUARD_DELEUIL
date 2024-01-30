using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtendedSerialPort;
using System.IO.Ports;
using System.Windows.Threading;
using System.CodeDom;

#pragma warning disable CS8618 // Un champ non-nullable doit contenir une valeur non-null lors de la fermeture du constructeur. Envisagez de déclarer le champ comme nullable.

namespace RobotInterface_CDN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        bool etatBouton;
        bool retour;
        byte etatLed;
        byte led_Number;
        private ReliableSerialPort serialPort1;
        private DispatcherTimer timerAffichage;
        Robot robot = new Robot();
        byte[] byteList;

        public object SerialPort1 { get; private set; }

        public MainWindow()
        {
            timerAffichage = new DispatcherTimer();
            timerAffichage.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerAffichage.Tick += TimerAffichage_Tick;
            timerAffichage.Start();

            serialPort1 = new ExtendedSerialPort.ReliableSerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
            serialPort1.OnDataReceivedEvent += SerialPort1_DataReceived;
            serialPort1.Open();
            InitializeComponent();
            etatBouton = true;
            retour = false;
            robot.receivedText = "";
            byteList = new byte[20];
            etatLed = 0;
            led_Number = 0;
            textBoxTest.Clear();

        }

        private void TimerAffichage_Tick(object? sender, EventArgs e)
        {
            while (robot.byteListReceived.Count > 0)
            {
                var c = robot.byteListReceived.Dequeue();
                DecodeMessage(c);

            }
        }

        public void SerialPort1_DataReceived(object? sender, DataReceivedArgs e)
        {
            for (int i = 0; i < e.Data.Length; i++)
            {
                robot.byteListReceived.Enqueue(e.Data[i]);

            }

        }

        private void textBoxEmission_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBoxReception_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void buttonEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            etatBouton = !etatBouton;
            retour = true;
            if (etatBouton == false)
            {
                buttonEnvoyer.Background = Brushes.RoyalBlue;
            }
            else
            {
                buttonEnvoyer.Background = Brushes.Beige;
            }
            SendMessage(retour);
        }

        private void textBoxEmission_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                retour = false;
                SendMessage(retour);
            }
            if (e.Key == Key.Delete)
            {
                textBoxTest.Clear();
            }
        }

        private void SendMessage(bool retour)
        {

            byte[] array = Encoding.ASCII.GetBytes(textBoxEmission.Text);
            UartEncodeAndSendMessage(0x0080, array.Length, array);
            textBoxEmission.Clear();
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            textBoxTest.Clear();
        }


        private byte CalculateChecksum(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            byte checksum = 0;
            checksum ^= 0xFE;
            checksum ^= (byte)(msgFunction >> 8);
            checksum ^= (byte)(msgFunction);
            checksum ^= (byte)(msgPayloadLength >> 8);
            checksum ^= (byte)(msgPayloadLength);

            for (int i = 0; i < msgPayloadLength; i++)
            {
                checksum ^= msgPayload[i];
            }

            return checksum;
        }

        void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            byte[] message = new byte[msgPayloadLength + 6];
            int pos = 0;
            message[pos++] = 0xFE;
            message[pos++] = (byte)(msgFunction >> 8);
            message[pos++] = (byte)msgFunction;
            message[pos++] = (byte)(msgPayloadLength >> 8);
            message[pos++] = (byte)msgPayloadLength;

            for (int i = 0; i < msgPayloadLength; i++)
            {
                message[pos++] = msgPayload[i];
            }

            message[pos] = CalculateChecksum(msgFunction, msgPayloadLength, msgPayload);

            serialPort1.Write(message, 0, message.Length);
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
                    {
                        rcvState = StateReception.FunctionMSB;
                        msgDecodedPayloadIndex = 0;
                    }
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
                    if (msgDecodedPayloadLength == 0)
                    {
                        rcvState = StateReception.CheckSum;
                    }
                    else if (msgDecodedPayloadLength > 1024)
                    {
                        rcvState = StateReception.Waiting;
                    }
                    else
                    {
                        rcvState = StateReception.Payload;
                    }
                    break;

                case StateReception.Payload:
                    msgDecodedPayload[msgDecodedPayloadIndex++] = c;
                    if (msgDecodedPayloadIndex >= msgDecodedPayloadLength)
                    {
                        rcvState = StateReception.CheckSum;
                    }
                    break;

                case StateReception.CheckSum:
                    byte receivedChecksum = c;
                    byte calculatedChecksum = CalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);

                    if (calculatedChecksum == receivedChecksum)
                    {
                        ProcessDecodedMessage(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
                    }
                    else
                    {
                    }
                    rcvState = StateReception.Waiting;
                    break;

                default:
                    rcvState = StateReception.Waiting;
                    break;
            }
        }

        void ProcessDecodedMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            if (msgFunction == 0x0080)
            {
                textBoxTest.Text += Encoding.ASCII.GetString(msgPayload);
                textBoxTest.Text += "\n";
            }
            else if (msgFunction == 0x0020)
            {
                textBoxTest.Text += "LED ORANGE : ";
                for (int i = 0; i < msgPayloadLength; i++)
                {
                    textBoxTest.Text += msgPayload[i].ToString("") + " ";
                }
                textBoxTest.Text += "\n";
            }
            else if (msgFunction == 0x0021)
            {
                textBoxTest.Text += "LED BLEUE : ";
                for (int i = 0; i < msgPayloadLength; i++)
                {
                    textBoxTest.Text += msgPayload[i].ToString("") + " ";
                }
                textBoxTest.Text += "\n";
            }
            else if (msgFunction == 0x0022)
            {
                textBoxTest.Text += "LED BLANCHE : ";
                for (int i = 0; i < msgPayloadLength; i++)
                {
                    textBoxTest.Text += msgPayload[i].ToString("") + " ";
                }
                textBoxTest.Text += "\n";
            }
            else if (msgFunction == 0x0030)
            {
                //Valeur Telemetre Extrem droit
                textBlockTelem_ED.Text = BitConverter.ToInt16(msgPayload, 0).ToString();
            }
            else if (msgFunction == 0x0031)
            {
                //Valeur Telemetre Droit
                textBlockTelem_D.Text = BitConverter.ToInt16(msgPayload, 0).ToString();
            }
            else if (msgFunction == 0x0032)
            {
                //Valeur Telemetre centre
                textBlockTelem_C.Text = BitConverter.ToInt16(msgPayload, 0).ToString();
            }
            else if (msgFunction == 0x0033)
            {
                //Valeur Telemetre gauche
                textBlockTelem_G.Text = BitConverter.ToInt16(msgPayload, 0).ToString();
            }
            else if (msgFunction == 0x0034)
            {
                //Valeur Telemetre Extrem gauche
                textBlockTelem_EG.Text = BitConverter.ToInt16(msgPayload, 0).ToString();
            }
            else if (msgFunction == 0x0040)
            {
                //Valeur vitesse moteur Gauche
                textVitesse_G.Text = BitConverter.ToInt16(msgPayload, 0).ToString();
                ProgVitesse_G.Value = BitConverter.ToInt16(msgPayload, 0);
            }
            else if (msgFunction == 0x0041)
            {
                //Valeur Vitesse moteur Droit
                textVitesse_D.Text = BitConverter.ToInt16(msgPayload, 0).ToString();
                ProgVitesse_D.Value = BitConverter.ToInt16(msgPayload, 0);

            }
            else if (msgFunction == 0x0061)
            {
                textBoxTest.Text += Encoding.ASCII.GetString(msgPayload) ;
            }

        }

        int etat_LED_Orange = 1;
        int etat_LED_Blanche = 1;
        int etat_LED_Bleue = 1;
        int mode = 0;
        byte[] message_LED = new byte[1];


        private void LED_Orange_Click(object sender, RoutedEventArgs e)
        {
            etat_LED_Orange = 1 - etat_LED_Orange;
            message_LED[0] = (byte)etat_LED_Orange;
            UartEncodeAndSendMessage(0x0020, 1, message_LED);
        }

        private void LED_Bleu_Click(object sender, RoutedEventArgs e)
        {
            etat_LED_Bleue = 1 - etat_LED_Bleue;
            message_LED[0] = (byte)etat_LED_Bleue;
            UartEncodeAndSendMessage(0x0021, 1, message_LED);
        }

        private void LED_Blanche_Click(object sender, RoutedEventArgs e)
        {
            etat_LED_Blanche = 1 - etat_LED_Blanche;
            message_LED[0] = (byte)etat_LED_Blanche;
            UartEncodeAndSendMessage(0x0022, 1, message_LED);
        }



    /*  A TESTER

        private void CheckBoxMode_Click(object sender, RoutedEventArgs e) // Changement de mode
        {
            byte[] mess_Mode = new byte[1];
            mode = 1 - mode;
            if (mode == 1)
            {
                labelAuto.Visibility = Visibility.Visible;
                labelManu.Visibility = Visibility.Hidden;
            }
            else
            {
                labelAuto.Visibility = Visibility.Hidden;
                labelManu.Visibility = Visibility.Visible;
            }
        }


        private void SliderV_droit_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) //changement Valeur de la vitesse moteur droit
        {
            byte[] mess_VD = new byte[1];
            mess_VD[0] = (byte)SliderV_droit.Value;
            textBoxVD_S.Text = SliderV_droit.Value.ToString();
            UartEncodeAndSendMessage(0x0041, mess_VD.Length, mess_VD);
        }

        private void SliderV_gauche_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) //changement Valeur de la vitesse moteur gauche
        {
            byte[] mess_VG = new byte[1];
            mess_VG[0] = (byte)SliderV_gauche.Value;
            textBoxVG_S.Text = SliderV_gauche.Value.ToString();
            UartEncodeAndSendMessage(0x0040, mess_VG.Length, mess_VG);
        }

    */
    }
}
