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
using System.Diagnostics;
using Syncfusion.UI.Xaml.Gauges;
using System.Numerics;
using Syncfusion.Windows.Shared;
using System.Windows.Media.Media3D;
using SciChart.Charting.Visuals;
using System.Threading;
using WpfOscilloscopeControl;
using System.Windows.Media.Effects;
using System.Windows.Controls.Primitives;
using static SciChart.Drawing.Utility.PointUtil;


namespace robotInterface

{
    public partial class MainWindow : Window
    {
        private ReliableSerialPort serialPort1;
        private bool isSerialPortAvailable = false;
        private DispatcherTimer timerDisplay;
        private Robot robot = new Robot();
        private SerialProtocolManager UARTProtocol = new SerialProtocolManager();

        private SolidColorBrush led1FillBeforeStop;
        private SolidColorBrush led2FillBeforeStop;
        private SolidColorBrush led3FillBeforeStop;
        private SolidColorBrush led1ForegroundBeforeStop;
        private SolidColorBrush led2ForegroundBeforeStop;
        private SolidColorBrush led3ForegroundBeforeStop;

        private DateTime lastToggleTime = DateTime.MinValue;



#pragma warning disable CS8618 
        public MainWindow()
#pragma warning restore CS8618
        {
            InitializeSerialPort();
            InitializeComponent();
            InitializeLedStates();

            timerDisplay = new DispatcherTimer();
            timerDisplay.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerDisplay.Tick += TimerDisplay_Tick;
            timerDisplay.Start();

            UARTProtocol.setRobot(robot);
            this.WindowStyle = WindowStyle.None;         // Désactive les styles Windows par défaut
            this.ResizeMode = ResizeMode.NoResize;       // Désactive le recadrage automatique de la fenêtre
            this.WindowState = WindowState.Maximized;    // Ouvre la fenêtre en plein écran automatiquement

            ToggleSwitch.IsChecked = true;

            oscilloSpeed.AddOrUpdateLine(1, 200, "Ligne 1");
            oscilloSpeed.ChangeLineColor(1, Color.FromRgb(0, 255, 0));

            oscilloPos.AddOrUpdateLine(2, 200, "Ligne 2");
            oscilloPos.ChangeLineColor(2, Color.FromRgb(0, 0, 255));
        }

        private void TimerDisplay_Tick(object? sender, EventArgs e)
        {

            /*
            Random rnd = new Random();
            robot.consigneGauche = (float)(rnd.NextDouble() * 200 - 100);
            robot.consigneDroite = (float)(rnd.NextDouble() * 200 - 100);
            */

            if (robot.receivedText != "")
            {
                textBoxReception.Text += robot.receivedText;
                robot.receivedText = "";
            }

            labelPositionXOdo.Content = "Position X\n{value} mm".Replace("{value}", robot.positionXOdo.ToString("F0"));
            labelPositionYOdo.Content = "Position Y\n{value} mm".Replace("{value}", robot.positionYOdo.ToString("F0"));
            labelAngle.Content = "Angle\n{value} rad".Replace("{value}", robot.angle.ToString("F0"));
            labelTimestamp.Content = "Timer\n{value} s".Replace("{value}", robot.timestamp.ToString("F1"));
            labelVitLin.Content = "Vitesse Linéaire\n{value} mm/ms".Replace("{value}", robot.vitLin.ToString("F0"));
            labelVitAng.Content = "Vitesse Angulaire\n{value} rad/ms".Replace("{value}", robot.vitAng.ToString("F0"));


            oscilloSpeed.AddPointToLine(1, robot.timestamp, robot.vitLin);
            oscilloPos.AddPointToLine(2, robot.positionXOdo, robot.positionYOdo);


            asservSpeedDisplay.UpdatePolarSpeedCorrectionGains(robot.pidLin.Kp, robot.pidAng.Kp, robot.pidLin.Ki, robot.pidAng.Ki, robot.pidLin.Kd, robot.pidAng.Kd);
            asservSpeedDisplay.UpdatePolarSpeedCorrectionLimits(robot.pidLin.erreurPmax, robot.pidAng.erreurPmax, robot.pidLin.erreurImax, robot.pidAng.erreurImax, robot.pidLin.erreurDmax, robot.pidAng.erreurDmax);
            asservSpeedDisplay.UpdatePolarSpeedCommandValues(robot.pidLin.cmdLin, robot.pidAng.cmdAng);
            asservSpeedDisplay.UpdatePolarSpeedConsigneValues(robot.pidLin.consigne, robot.pidAng.consigne);
            asservSpeedDisplay.UpdatePolarSpeedCorrectionValues(robot.pidLin.corrP, robot.pidAng.corrP, robot.pidLin.corrI, robot.pidAng.corrI, robot.pidLin.corrD, robot.pidAng.corrD);
            asservSpeedDisplay.UpdatePolarSpeedErrorValues(robot.pidLin.erreur, robot.pidAng.erreur);
            asservSpeedDisplay.UpdatePolarOdometrySpeed(robot.vitLin, robot.vitAng);
            asservSpeedDisplay.UpdateDisplay();

            while (robot.stringListReceived.Count != 0)
            {
                // byte current = robot.byteListReceived.Dequeue();
                textBoxReception.Text += robot.stringListReceived.Dequeue();
                // textBoxReception.Text += Convert.ToChar(current).ToString();
            }

            updateTelemetreGauges();
            updateTelemetreBoxes();
            updateSpeedGauges();

        }

        private void InitializeSerialPort()
        {
            string comPort = "COM3";
            if (SerialPort.GetPortNames().Contains(comPort))
            {
                serialPort1 = new ReliableSerialPort(comPort, 115200, Parity.None, 8, StopBits.One);
                serialPort1.OnDataReceivedEvent += SerialPort1_DataReceived;
                try
                {
                    serialPort1.Open();
                    isSerialPortAvailable = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening serial port: " + ex.Message);
                }
            }
            //else MessageBox.Show("Port doesn't exist");
        }

        public void SerialPort1_DataReceived(object? sender, DataReceivedArgs e)
        {
            for (int i = 0; i < e.Data.Length; i++)
            {
                UARTProtocol.DecodeMessage(e.Data[i]);
            }
        }

        private bool sendMessage(bool key)
        {
            if (textBoxEmission.Text == "\r\n" || textBoxEmission.Text == "") return false;

            byte[] payload = new byte[textBoxEmission.Text.Length];

            for (int i = 0; i < textBoxEmission.Text.Length; i++)
                payload[i] = (byte)textBoxEmission.Text[i];

            serialPort1.SendMessage(this, payload);
            textBoxEmission.Text = "";
            return true;
        }

        private void btnEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            sendMessage(false);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            textBoxReception.Text = "";
        }

        private void textBoxEmission_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!sendMessage(true))
                {
                    textBoxEmission.Text = "";
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }

            if (e.Key == Key.Tab)
            {
                tabs.SelectedIndex = (tabs.SelectedIndex == 0) ? 1 : 0;

                e.Handled = true;
            }

        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {

            byte[] byteList = new byte[20];
            for (int i = 19; i >= 0; i--)
                byteList[i] = (byte)(2 * i);

            serialPort1.Write(byteList, 0, 20);
        }

        // Calculer les transformations des rectangles (obstacles)
        private Vector2 getBTTranslationVector(double angle, double scaleCoef, double distance)
        {
            Vector2 translateVector = new Vector2();
            translateVector.X = (float)(Math.Cos((Math.PI / 180) * (270 + angle)) * distance * scaleCoef);
            translateVector.Y = (float)(Math.Sin((Math.PI / 180) * (270 + angle)) * distance * scaleCoef);
            return translateVector;
        }

        // Gestion de l'affichage des obsctacles par rapport au robot
        private void updateTelemetreBoxes()
        {
            var scaleCoef = 0.5;

            var angle = -47.703;
            TransformGroup customTGELeft = new TransformGroup();
            customTGELeft.Children.Add(new RotateTransform(angle));

            Vector2 translationVector = getBTTranslationVector(angle, scaleCoef, robot.distanceTelemetreMelenchon);
            customTGELeft.Children.Add(new TranslateTransform(translationVector.X, translationVector.Y));

            boxTeleELeft.RenderTransform = customTGELeft;


            // -------------------------------------------------
            angle = -24.786;
            TransformGroup customTGLeft = new TransformGroup();
            customTGLeft.Children.Add(new RotateTransform(angle));
            translationVector = getBTTranslationVector(angle, scaleCoef, robot.distanceTelemetreGauche);
            customTGLeft.Children.Add(new TranslateTransform(translationVector.X, translationVector.Y));

            boxTeleLeft.RenderTransform = customTGLeft;

            // -------------------------------------------------

            angle = 0;
            TransformGroup customTGCenter = new TransformGroup();
            translationVector = getBTTranslationVector(angle, scaleCoef, robot.distanceTelemetreCentre);
            customTGCenter.Children.Add(new TranslateTransform(translationVector.X, translationVector.Y));

            boxTeleCenter.RenderTransform = customTGCenter;

            // -------------------------------------------------
            angle = 24.786;

            TransformGroup customTGRight = new TransformGroup();
            customTGRight.Children.Add(new RotateTransform(angle));
            translationVector = getBTTranslationVector(angle, scaleCoef, robot.distanceTelemetreDroit);
            customTGRight.Children.Add(new TranslateTransform(translationVector.X, translationVector.Y));

            boxTeleRight.RenderTransform = customTGRight;

            // -------------------------------------------------
            angle = 47.703;

            TransformGroup customTGERight = new TransformGroup();
            customTGERight.Children.Add(new RotateTransform(angle));
            translationVector = getBTTranslationVector(angle, scaleCoef, robot.distanceTelemetreLePen);
            customTGERight.Children.Add(new TranslateTransform(translationVector.X, translationVector.Y));

            boxTeleERight.RenderTransform = customTGERight;
        }


        private void updateSpeedGauges()
        {
            updateGaugePointer(LeftGauge, robot.consigneGauche);
            updateGaugePointer(RightGauge, robot.consigneDroite);
        }

        // Gestion des jauges pour les vitesses
        private void updateGaugePointer(SfCircularGauge gauge, double value)
        {
            if (gauge.Scales[0].Pointers.Count == 0)
            {
                CircularPointer circularPointer = new CircularPointer
                {
                    PointerType = PointerType.NeedlePointer,
                    Value = value,
                    NeedleLengthFactor = 0.5,
                    NeedlePointerType = NeedlePointerType.Triangle,
                    PointerCapDiameter = 12,
                    NeedlePointerStroke = new BrushConverter().ConvertFrom("#757575") as SolidColorBrush,
                    KnobFill = new BrushConverter().ConvertFrom("#757575") as SolidColorBrush,
                    KnobStroke = new BrushConverter().ConvertFrom("#757575") as SolidColorBrush,
                    NeedlePointerStrokeThickness = 7
                };

                gauge.Scales[0].Pointers.Add(circularPointer);
            }
            else
            {
                (gauge.Scales[0].Pointers[0] as CircularPointer).Value = value;
            }
        }

        // Gestion des jauges verticales pour les distances des télémètres
        private void updateTelemetreGauges()
        {
            telemetreMelenchonRange.EndValue = robot.distanceTelemetreMelenchon;
            telemetreMelenchonRange.RangeStroke = ChooseColor(robot.distanceTelemetreMelenchon);
            textTelemetreMelenchon.Text = robot.distanceTelemetreMelenchon.ToString("F0") + " cm";

            telemetreGaucheRange.EndValue = robot.distanceTelemetreGauche;
            telemetreGaucheRange.RangeStroke = ChooseColor(robot.distanceTelemetreGauche);
            textTelemetreGauche.Text = robot.distanceTelemetreGauche.ToString("F0") + " cm";

            telemetreCentreRange.EndValue = robot.distanceTelemetreCentre;
            telemetreCentreRange.RangeStroke = ChooseColor(robot.distanceTelemetreCentre);
            textTelemetreCentre.Text = robot.distanceTelemetreCentre.ToString("F0") + " cm";

            telemetreDroitRange.EndValue = robot.distanceTelemetreDroit;
            telemetreDroitRange.RangeStroke = ChooseColor(robot.distanceTelemetreDroit);
            textTelemetreDroit.Text = robot.distanceTelemetreDroit.ToString("F0") + " cm";

            telemetreLePenRange.EndValue = robot.distanceTelemetreLePen;
            telemetreLePenRange.RangeStroke = ChooseColor(robot.distanceTelemetreLePen);
            textTelemetreLePen.Text = robot.distanceTelemetreLePen.ToString("F0") + " cm";
        }

        private Brush ChooseColor(float distance)
        {
            if (distance < 20)
            {
                return Brushes.Red; // Rouge pour les valeurs inférieures à 20
            }
            else if (distance < 40)
            {
                return Brushes.Orange; // Orange pour les valeurs entre 20 et 40
            }
            else
            {
                return Brushes.Green; // Vert pour les valeurs supérieures à 40
            }
        }


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove(); // Permet de déplacer la fenêtre
        }


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized; // Minimise la fenêtre
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal; // Restaure la fenêtre si elle est maximisée
            }
            else
            {

                this.WindowState = WindowState.Maximized; // Maximise la fenêtre si elle n'est pas maximisée
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Ferme la fenêtre
        }


        // Lors d'un clic, bascule l'état
        private void EllipseLed_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ellipse = sender as Ellipse;
            if (ellipse != null)
            {
                byte numeroLed = Convert.ToByte(ellipse.Tag);
                bool isLedOn = ellipse.Fill.ToString() == Brushes.Black.ToString();

                ToggleLed(ellipse, numeroLed, !isLedOn);
            }
        }

        // Logique de gestion des couleurs des LEDs (ON/OFF)
        private void ToggleLed(Ellipse ellipse, byte numeroLed, bool isLedOn)
        {
            var etat = Convert.ToByte(isLedOn);
            SolidColorBrush newColor = Brushes.Black;
            SolidColorBrush textColor = Brushes.White;

            TextBlock? textBlockAssociated = null;

            switch (numeroLed)
            {
                case 0:
                    newColor = isLedOn ? Brushes.Black : Brushes.White;
                    textColor = isLedOn ? Brushes.White : Brushes.Black;
                    textBlockAssociated = textBlockLed1;
                    break;
                case 1:
                    newColor = isLedOn ? Brushes.Black : Brushes.Blue;
                    textColor = isLedOn ? Brushes.Blue : Brushes.White;
                    textBlockAssociated = textBlockLed2;
                    break;
                case 2:
                    newColor = isLedOn ? Brushes.Black : Brushes.Orange;
                    textColor = isLedOn ? Brushes.Orange : Brushes.White;
                    textBlockAssociated = textBlockLed3;
                    break;
            }

            ellipse.Fill = newColor;
            if (textBlockAssociated != null)
            {
                textBlockAssociated.Foreground = textColor;
            }

            var textBlock = FindTextBlockForLed(ellipse);

            if (textBlock != null)
            {
                textBlock.Foreground = textColor;
            }


            // Mise à jour des LEDs sur le robot
            UpdateVoyants();

            if (isSerialPortAvailable)
            {
                byte[] rawData = UARTProtocol.UartEncode(new SerialCommandLED(numeroLed, etat));
                serialPort1.Write(rawData, 0, rawData.Length);
            }

        }

        // Identification des LEDs
        private TextBlock? FindTextBlockForLed(Ellipse ellipse)
        {
            if (ellipse.Parent is Grid grid)
            {
                int column = Grid.GetColumn(ellipse);

                foreach (var child in grid.Children)
                {
                    if (child is TextBlock textBlock && Grid.GetColumn(textBlock) == column)
                    {
                        return textBlock;
                    }
                }
            }
            return null;
        }

        private void InitializeLedStates()
        {
            // LED Bleue - ON
            ellipseLed2.Fill = Brushes.Blue;
            textBlockLed2.Foreground = Brushes.White;

            // LEDs Blanche et Orange - OFF
            ellipseLed1.Fill = Brushes.Black;
            textBlockLed1.Foreground = Brushes.White;
            ellipseLed3.Fill = Brushes.Black;
            textBlockLed3.Foreground = Brushes.Orange;

            UpdateVoyants();
        }

        private void TurnOffAllLeds()
        {
            // LED 1 - OFF
            ellipseLed1.Fill = Brushes.Black;
            if (textBlockLed1 != null) textBlockLed1.Foreground = Brushes.White; // Ajustez si nécessaire

            // LED 2 - OFF
            ellipseLed2.Fill = Brushes.Black;
            if (textBlockLed2 != null) textBlockLed2.Foreground = Brushes.White; // Ajustez si nécessaire

            // LED 3 - OFF
            ellipseLed3.Fill = Brushes.Black;
            if (textBlockLed3 != null) textBlockLed3.Foreground = Brushes.White; // Ajustez si nécessaire

            UpdateVoyants();
        }

        private void SaveLedStatesBeforeStop()
        {
            // Sauvegarde des états de remplissage et de texte pour chaque LED
            led1FillBeforeStop = (SolidColorBrush)ellipseLed1.Fill;
            led2FillBeforeStop = (SolidColorBrush)ellipseLed2.Fill;
            led3FillBeforeStop = (SolidColorBrush)ellipseLed3.Fill;
            led1ForegroundBeforeStop = (SolidColorBrush)textBlockLed1.Foreground;
            led2ForegroundBeforeStop = (SolidColorBrush)textBlockLed2.Foreground;
            led3ForegroundBeforeStop = (SolidColorBrush)textBlockLed3.Foreground;
        }

        private void RestoreLedStates()
        {
            SetLedState(ellipseLed1, led1FillBeforeStop, led1ForegroundBeforeStop);
            SetLedState(ellipseLed2, led2FillBeforeStop, led2ForegroundBeforeStop);
            SetLedState(ellipseLed3, led3FillBeforeStop, led3ForegroundBeforeStop);
            UpdateVoyants();
        }


        private void SetLedState(Ellipse led, SolidColorBrush fill, SolidColorBrush foreground)
        {
            // Mise à jour de la couleur de remplissage de la LED
            led.Fill = fill;

            // Trouver le TextBlock associé à la LED et mettre à jour sa couleur de texte
            var textBlock = FindTextBlockForLed(led);
            if (textBlock != null)
            {
                textBlock.Foreground = foreground;
            }
        }



        // Gestion de la couleur des leds
        private void UpdateVoyants()
        {
            voyantLed1.Fill = ellipseLed1.Fill == Brushes.Black ? Brushes.Black : Brushes.White;
            voyantLed2.Fill = ellipseLed2.Fill == Brushes.Black ? Brushes.Black : Brushes.Blue;
            voyantLed3.Fill = ellipseLed3.Fill == Brushes.Black ? Brushes.Black : Brushes.Orange;
        }

        private bool isStopBtnPressed = false;
        private void EllipseStopBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Inverser l'état du bouton à chaque clic
            isStopBtnPressed = !isStopBtnPressed;

            if (isStopBtnPressed)
            {

                SaveLedStatesBeforeStop();
                TurnOffAllLeds();
                this.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/assets/STOPbackground.png")));


                scaleTransform.ScaleX = 0.95;
                scaleTransform.ScaleY = 0.95;
                shadowEffect.ShadowDepth = 1;
                shadowEffect.BlurRadius = 5;

                var encodedMessage = UARTProtocol.UartEncode(new SerialCommandText("STOP"));
                serialPort1.Write(encodedMessage, 0, encodedMessage.Length);
            }
            else
            {

                RestoreLedStates();
                this.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/assets/background.png")));


                scaleTransform.ScaleX = 1.0;
                scaleTransform.ScaleY = 1.0;
                shadowEffect.ShadowDepth = 5;
                shadowEffect.BlurRadius = 10;

                var encodedMessage = UARTProtocol.UartEncode(new SerialCommandText("GO"));
                serialPort1.Write(encodedMessage, 0, encodedMessage.Length);
            }
        }

        // private int mode = 0;

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            // Vérifier si le délai minimal est respecté
            if ((DateTime.Now - lastToggleTime).TotalMilliseconds < 200)
            {
                return;
            }
            lastToggleTime = DateTime.Now;

            if (sender is ToggleButton toggleButton)
            {
                toggleButton.Background = new SolidColorBrush(Color.FromRgb(0, 0, 255)); // Bleu
                MoveIndicator(toggleButton, true);
            }

            // Allumer la LED bleue et éteindre les autres
            SetLedState(ellipseLed2, Brushes.Blue, Brushes.White);
            SetLedState(ellipseLed1, Brushes.Black, Brushes.White);
            SetLedState(ellipseLed3, Brushes.Black, Brushes.White);
            UpdateVoyants();

            var encodedMessage = UARTProtocol.UartEncode(new SerialCommandText("asservDisabled"));
            serialPort1.Write(encodedMessage, 0, encodedMessage.Length);
        }


        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            // Vérifier si le délai minimal est respecté
            if ((DateTime.Now - lastToggleTime).TotalMilliseconds < 200)
            {
                return;
            }
            lastToggleTime = DateTime.Now;

            if (sender is ToggleButton toggleButton)
            {
                toggleButton.Background = new SolidColorBrush(Color.FromRgb(255, 128, 0)); // Orange
                MoveIndicator(toggleButton, false);
            }

            // Allumer les LEDs blanche et orange, éteindre la LED bleue
            SetLedState(ellipseLed1, Brushes.White, Brushes.Black);
            SetLedState(ellipseLed3, Brushes.Orange, Brushes.White);
            SetLedState(ellipseLed2, Brushes.Black, Brushes.White);
            UpdateVoyants();

            byte[] rawDataLin = UARTProtocol.UartEncode(new SerialCommandSetPID(Pid.PID_LIN, 10, 10, 10, 10, 10, 10));
            byte[] rawDataAng = UARTProtocol.UartEncode(new SerialCommandSetPID(Pid.PID_ANG, 10, 10, 10, 10, 10, 10));

            serialPort1.Write(rawDataLin, 0, rawDataLin.Length);
            serialPort1.Write(rawDataAng, 0, rawDataAng.Length);
        }


        private void MoveIndicator(ToggleButton toggleButton, bool isChecked)
        {
            if (toggleButton.Template.FindName("toggleIndicator", toggleButton) is Ellipse toggleIndicator)
            {
                toggleIndicator.VerticalAlignment = isChecked ? VerticalAlignment.Top : VerticalAlignment.Bottom;
            }
        }
    }
}
