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

namespace RobotInterface_COQUARD_NOEL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        bool etatBouton;
        bool retour;
        byte etatLed ;
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
            etatLed = 0 ;
            led_Number = 0;
            textBoxTest.Clear();

        }

        private void TimerAffichage_Tick(object? sender, EventArgs e)
        {
            while (robot.byteListReceived.Count > 0)
            {
                var c = robot.byteListReceived.Dequeue();
                //textBoxReception.Text += "0x" + c.ToString("X2") + " ";
                DecodeMessage(c);
                
            }
        }

        public void SerialPort1_DataReceived(object? sender, DataReceivedArgs e)
        {
            for (int i = 0; i < e.Data.Length; i++)
            {
                robot.byteListReceived.Enqueue(e.Data[i]);

            }
            
            //robot.receivedText += Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);
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
            if (retour)
            {
                ///* textBoxEmission.Text += "\r\n"*/;
            }
            //textBoxReception.Text += "Reçu : " + textBoxEmission.Text ;
            byte[] array = Encoding.ASCII.GetBytes(textBoxEmission.Text);
            UartEncodeAndSendMessage(0x0080, array.Length, array);
            //serialPort1.WriteLine(textBoxEmission.Text.Substring(0, textBoxEmission.Text.Length - 2));
            textBoxEmission.Clear();
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            textBoxTest.Clear();
        }

        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            //Random rnd = new Random();
            //int led = rnd.Next(15);
            //int etatLed = rnd.Next(2);
            //int tlm_G = rnd.Next(101);
            //int tlm_D = rnd.Next(101);
            //int tlm_C = rnd.Next(101);
            //int v_G = rnd.Next(101);
            //int v_D = rnd.Next(101);
            //int i = 0;
            //int j = rnd.Next(5, 16);

            //byte[] message_Led = new byte[2];
            //message_Led[0] = (byte)led;
            //message_Led[1] = (byte)etatLed;

            //byte[] message_Texte = new byte[j];
            //for (i = 0; i < j; i++)
            //{
            //    message_Texte[i] = (byte)(rnd.Next(97, 123));
            //}
            //byte[] message_Vitesse = new byte[2];
            //message_Vitesse[0] = (byte)v_G;
            //message_Vitesse[1] = (byte)v_D;

            //byte[] message_Telemetre = new byte[3];
            //message_Telemetre[0] = (byte)tlm_G;
            //message_Telemetre[1] = (byte)tlm_C;
            //message_Telemetre[2] = (byte)tlm_D;

            //UartEncodeAndSendMessage(0x0080, j, message_Texte);
            //UartEncodeAndSendMessage(0x0020, 2, message_Led);
            //UartEncodeAndSendMessage(0x0030, 3, message_Telemetre);
            //UartEncodeAndSendMessage(0x0040, 2, message_Vitesse);


            //i = 0;
            //for (i = 0; i < 20; i++)
            //{
            //    byteList[i] = (byte)(2 * i);
            //}
            //UartEncodeAndSendMessage(0x0080, i, byteList);
            //serialPort1.Write(Encoding.UTF8.GetString(byteList, 0, 20));
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
                    msgDecodedFunction = c<<8;
                    rcvState = StateReception.FunctionLSB;
                    break;

                case StateReception.FunctionLSB:
                    msgDecodedFunction |= c;
                    rcvState = StateReception.PayloadLengthMSB;
                    break;

                case StateReception.PayloadLengthMSB:
                    msgDecodedPayloadLength = c<<8;
                    rcvState = StateReception.PayloadLengthLSB;
                    break;

                case StateReception.PayloadLengthLSB:
                    msgDecodedPayloadLength |= c;
                    msgDecodedPayload= new byte[msgDecodedPayloadLength] ;
                    if (msgDecodedPayloadLength == 0)
                    {
                        rcvState = StateReception.CheckSum;
                    }
                    else if (msgDecodedPayloadLength>1024)
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
                        //textBoxReception.Text += "\nBad CheckSum\n" ;
                    }
                    rcvState = StateReception.Waiting;
                    break;

                default:
                    rcvState = StateReception.Waiting;
                    break;
            }
        }

        void ProcessDecodedMessage(int msgFunction,int msgPayloadLength, byte[] msgPayload)
        {


            if (msgFunction==0x0080)
            {
                textBoxTest.Text += Encoding.ASCII.GetString(msgPayload) ;
                textBoxTest.Text += "\n";
            }
            else if (msgFunction==0x0020)
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
                //textBlockTelem_ED.Text += "TELEM_ED : ";
                textBlockTelem_ED.Text = BitConverter.ToInt16(msgPayload, 0).ToString();
            }
            else if (msgFunction == 0x0031)
            {
                //textBlockTelem_D.Text += "TELEM_D : ";
                textBlockTelem_D.Text = BitConverter.ToInt16(msgPayload, 0).ToString();
            }
            else if (msgFunction == 0x0032)
            {
                //textBlockTelem_C.Text += "TELEM_C : ";
                textBlockTelem_C.Text = BitConverter.ToInt16(msgPayload, 0).ToString();
            }
            else if (msgFunction == 0x0033)
            {
                //textBlockTelem_G.Text += "TELEM_G : ";
                textBlockTelem_G.Text = BitConverter.ToInt16(msgPayload, 0).ToString();
            }
            else if (msgFunction == 0x0034)
            {
               // textBlockTelem_EG.Text += "TELEM_EG : ";
                textBlockTelem_EG.Text = BitConverter.ToInt16(msgPayload, 0).ToString();
               
            }
            else if (msgFunction == 0x0040)
            {
                textBoxTest.Text += "VIT GAUCHE : ";
                textBoxTest.Text += BitConverter.ToInt16(msgPayload, 0).ToString();
                textBoxTest.Clear();
            }
            else if (msgFunction == 0x0041)
            {
                textBoxTest.Text += "VIT DROITE : ";
                textBoxTest.Text += BitConverter.ToInt16(msgPayload, 0).ToString();
                textBoxTest.Text += "\n";
            }

        }

        int etat_LED_Orange = 1 ;
        int etat_LED_Blanche = 1;
        int etat_LED_Bleue = 1;
        byte[] message_LED = new byte[1];

        private void buttonLED_O_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonLED_BLC_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonLED_B_Click(object sender, RoutedEventArgs e)
        {
            
        }


        private void buttonVit_G_Click(object sender, RoutedEventArgs e)
        {
            byte[] array = Encoding.ASCII.GetBytes(textBoxEmission.Text);
            UartEncodeAndSendMessage(0x0040, 1, array);
        }

        private void buttonVit_D_Click(object sender, RoutedEventArgs e)
        {
            byte[] array = Encoding.ASCII.GetBytes(textBoxEmission.Text);
            UartEncodeAndSendMessage(0x0041, array.Length, array);
        }




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
    }
}
