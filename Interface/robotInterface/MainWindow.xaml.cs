﻿using ExtendedSerialPort;
using Gma.System.MouseKeyHook;
using SciChart.Charting.Visuals;
using Syncfusion.UI.Xaml.Gauges;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text.RegularExpressions;


namespace robotInterface
{
    public partial class MainWindow : Window
    {
        private ReliableSerialPort serialPort1;
        private bool isSerialPortAvailable = false;
        private DispatcherTimer timerDisplay;
        private Robot robot = new Robot();
        private SerialProtocolManager UARTProtocol = new SerialProtocolManager();
        private TrajectoryManager trajectoryManager = new TrajectoryManager();


        private SolidColorBrush led1FillBeforeStop;
        private SolidColorBrush led2FillBeforeStop;
        private SolidColorBrush led3FillBeforeStop;
        private SolidColorBrush led1ForegroundBeforeStop;
        private SolidColorBrush led2ForegroundBeforeStop;
        private SolidColorBrush led3ForegroundBeforeStop;

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        private bool isLaptop = true;
        private bool isSimulation = false;

        private bool isWaypointSent = false;
        private bool isHooking = false;
        private bool isMoving = false;
        private long lastWaypointTimeMillis = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSerialPort();
            InitializeLedStates();

            timerDisplay = new DispatcherTimer();
            timerDisplay.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerDisplay.Tick += TimerDisplay_Tick;
            timerDisplay.Start();

            UARTProtocol.setRobot(robot);
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowState = WindowState.Maximized;

            tabs.SelectedItem = tabPositionnement;

            oscilloLinearSpeed.AddOrUpdateLine(1, 200, "Ligne 2");
            oscilloLinearSpeed.ChangeLineColor(1, Color.FromRgb(0, 255, 0));

            oscilloAngularSpeed.AddOrUpdateLine(1, 200, "Ligne 1");
            oscilloAngularSpeed.ChangeLineColor(1, Color.FromRgb(0, 0, 255));

            oscilloPos.AddOrUpdateLine(2, 200, "Ligne 1");
            oscilloPos.ChangeLineColor(2, Color.FromRgb(255, 0, 0));

            IKeyboardMouseEvents m_GlobalHook;

            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyPress += GlobalHookKeyPress;

            InitMovingRobotPosition();
            if (isMoving) UpdateFeedbackWaypoint();

            InitializeTrajectoryUpdateTimer();

        }

        private void TimerDisplay_Tick(object? sender, EventArgs e)
        {
            if (robot.receivedText != "")
            {
                textBoxReception.Text += robot.receivedText;
                robot.receivedText = "";
            }

            labelPositionXOdo.Content = "Position X\n{value} m".Replace("{value}", robot.positionXOdo.ToString("F3"));
            labelPositionYOdo.Content = "Position Y\n{value} m".Replace("{value}", robot.positionYOdo.ToString("F3"));
            labelAngle.Content = "Angle\n{value} rad".Replace("{value}", robot.angle.ToString("F2"));
            labelTimestamp.Content = "Timer\n{value} s".Replace("{value}", robot.timestamp.ToString("F1"));
            labelVitLin.Content = "Vitesse Linéaire\n{value} m/ms".Replace("{value}", robot.vitLin.ToString("F3"));
            labelVitAng.Content = "Vitesse Angulaire\n{value} rad/ms".Replace("{value}", robot.vitAng.ToString("F2"));

            oscilloAngularSpeed.AddPointToLine(1, robot.timestamp, robot.vitAng);
            oscilloLinearSpeed.AddPointToLine(1, robot.timestamp, robot.vitLin);
            oscilloPos.AddPointToLine(2, robot.positionXOdo, robot.positionYOdo);

            asservSpeedDisplay.UpdatePolarSpeedCorrectionGains(robot.pidLin.Kp, robot.pidAng.Kp, robot.pidLin.Ki, robot.pidAng.Ki, robot.pidLin.Kd, robot.pidAng.Kd);
            asservSpeedDisplay.UpdatePolarSpeedCorrectionLimits(robot.pidLin.erreurPmax, robot.pidAng.erreurPmax, robot.pidLin.erreurImax, robot.pidAng.erreurImax, robot.pidLin.erreurDmax, robot.pidAng.erreurDmax);
            asservSpeedDisplay.UpdatePolarSpeedCommandValues(robot.pidLin.cmdLin, robot.pidAng.cmdAng);
            asservSpeedDisplay.UpdatePolarSpeedConsigneValues(robot.pidLin.consigne, robot.pidAng.consigne);
            asservSpeedDisplay.UpdatePolarSpeedCorrectionValues(robot.pidLin.corrP, robot.pidAng.corrP, robot.pidLin.corrI, robot.pidAng.corrI, robot.pidLin.corrD, robot.pidAng.corrD);
            asservSpeedDisplay.UpdatePolarSpeedErrorValues(robot.pidLin.erreur, robot.pidAng.erreur);
            asservSpeedDisplay.UpdatePolarOdometrySpeed(robot.vitLin, robot.vitAng);
            asservSpeedDisplay.UpdateDisplay();

            labelDistanceToTarget.Content = "Distance à la cible : {value} m".Replace("{value}", robot.ghost.distanceToTarget.ToString("F2"));
            labelAngleToTarget.Content = "Angle à la cible : {value} rad".Replace("{value}", robot.ghost.angleToTarget.ToString("F2"));

            UpdateFeedbackWaypoint();

            while (robot.stringListReceived.Count != 0)
            {
                textBoxReception.Text += robot.stringListReceived.Dequeue();
            }

            UpdateTelemetreGauges();
            UpdateTelemetreBoxes();
            UpdateSpeedGauges();
        }

        private void InitializeSerialPort()
        {
            string comPort = "COM7";

            if (SerialPort.GetPortNames().Contains(comPort))
            {
                serialPort1 = new ReliableSerialPort(comPort, 115200, Parity.None, 8, StopBits.One);
                serialPort1.OnDataReceivedEvent += SerialPort1_DataReceived;
                try
                {
                    serialPort1.Open();
                    isSerialPortAvailable = true;
                }
                catch (System.IO.IOException ex)
                {
                    Debug.WriteLine($"IOException lors de l'utilisation du port série: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Impossible d'ouvrir le port série : " + ex.Message);
                    MessageBox.Show("Impossible d'ouvrir le port série : " + ex.Message);
                    return;
                }
            }
            else
            {
                if (!isSimulation)
                {
                    MessageBoxResult result = MessageBox.Show("Le port COM n'existe pas accessible, voulez-vous activer le mode Simulation ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    isSimulation = (result == MessageBoxResult.Yes);

                    if (result == MessageBoxResult.No)
                    {
                        MessageBox.Show("Veuillez changer le numéro de port COM ou activer le mode Simulation", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Shutdown();
                    }
                }
            }

            // Appliquer la configuration de la grille basée sur le type d'appareil
            ApplyGridConfiguration(gridSupervision,
                isLaptop ? new List<double> { 71, 80, 95, 85, 115, 100, 120, 180, 85 } : new List<double> { 76, 91, 95, 93, 105, 100, 138, 199, 85 },
                isLaptop ? new List<double> { 66, 140, 34, 146, 40, 146, 35, 539, 62, 544, 65 } : new List<double> { 106, 156, 38, 148, 42, 146, 40, 530, 66, 538, 65 });

            ApplyGridConfiguration(gridAsservissement,
                isLaptop ? new List<double> { 71, 159, 63, 163, 65, 415, 70 } : new List<double> { 77, 167, 66, 172, 67, 437, 66 },
                isLaptop ? new List<double> { 66, 234, 67, 506, 67.8, 810.5 } : new List<double> { 104, 282, 72, 462, 69, 820 });

            ApplyGridConfiguration(gridPositionnement,
                isLaptop ? new List<double> { 71, 286, 67, 510, 70 } : new List<double> { 76, 301, 71, 505, 70 },
                isLaptop ? new List<double> { 66, 606, 65, 138, 67.8, 266, 68, 473.3 } : new List<double> { 104, 603, 68, 144, 73, 280, 60, 474 });

            ApplyCanvasConfiguration(isLaptop);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------- UI SETTINGS
        #region UI SETTINGS
        private void ApplyGridConfiguration(Grid targetGrid, List<double> rowHeights, List<double> columnWidths)
        {
            targetGrid.RowDefinitions.Clear();
            foreach (var height in rowHeights)
            {
                targetGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(height) });
            }

            targetGrid.ColumnDefinitions.Clear();
            foreach (var width in columnWidths)
            {
                targetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width) });
            }
        }

        private void ApplyCanvasConfiguration(bool isLaptop)
        {
            if (isLaptop)
            {
                // Configuration pour mode PC portable
                navCanva.Margin = new Thickness(-5.5, -3, 0, 2.7);
                closeButton.Content = " X";
                closeButton.Width = 37.5;
                maximizeRestoreButton.Content = "❐";
                maximizeRestoreButton.Width = 36;
                minimizeButton.Content = "—";
                minimizeButton.Width = 34.5;
                closeButton.SetValue(Canvas.LeftProperty, 1791.0);
                maximizeRestoreButton.SetValue(Canvas.LeftProperty, 1759.0);
                minimizeButton.SetValue(Canvas.LeftProperty, 1727.0);
            }
            else
            {
                // Configuration pour mode E105
                navCanva.Margin = new Thickness(84, -3, 0, 2.7);
                closeButton.Content = "  X";
                closeButton.Width = 43;
                maximizeRestoreButton.Content = "❐";
                maximizeRestoreButton.Width = 40;
                minimizeButton.Content = "—";
                minimizeButton.Width = 36;
                closeButton.SetValue(Canvas.LeftProperty, 1791.0);
                maximizeRestoreButton.SetValue(Canvas.LeftProperty, 1759.0);
                minimizeButton.SetValue(Canvas.LeftProperty, 1727.0);
            }
        }

        public void SerialPort1_DataReceived(object? sender, DataReceivedArgs e)
        {
            for (int i = 0; i < e.Data.Length; i++)
            {
                UARTProtocol.DecodeMessage(e.Data[i]);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Close();
            }

            if (e.Key == System.Windows.Input.Key.Tab)
            {
                tabs.SelectedIndex = (tabs.SelectedIndex == 0) ? 1 : 0;

                e.Handled = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Multiply) // Touche "*" sur le pavé numérique
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                }
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
        #endregion


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------- ONGLET 1
        #region ONGLET 1 
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
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!sendMessage(true))
                {
                    textBoxEmission.Text = "";
                }
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {

            byte[] byteList = new byte[20];
            for (int i = 19; i >= 0; i--)
                byteList[i] = (byte)(2 * i);

            if (!isSimulation) serialPort1.Write(byteList, 0, 20);
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
        private void UpdateTelemetreBoxes()
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
        private void UpdateSpeedGauges()
        {
            UpdateGaugePointer(LeftGauge, robot.consigneGauche);
            UpdateGaugePointer(RightGauge, robot.consigneDroite);
        }

        // Gestion des jauges pour les vitesses
        private void UpdateGaugePointer(SfCircularGauge gauge, double value)
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
        private void UpdateTelemetreGauges()
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

            UpdateVoyants();

            if (isSerialPortAvailable)
            {
                byte[] rawData = UARTProtocol.UartEncode(new SerialCommandLED(numeroLed, etat));
                if (!isSimulation) serialPort1.Write(rawData, 0, rawData.Length);
            }
        }

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
            //ToggleSwitch.IsChecked = true;
        }

        private void TurnOffAllLeds()
        {
            ellipseLed1.Fill = Brushes.Black;
            if (textBlockLed1 != null) textBlockLed1.Foreground = Brushes.White;

            ellipseLed2.Fill = Brushes.Black;
            if (textBlockLed2 != null) textBlockLed2.Foreground = Brushes.White;

            ellipseLed3.Fill = Brushes.Black;
            if (textBlockLed3 != null) textBlockLed3.Foreground = Brushes.White;

            UpdateVoyants();
        }

        private void SaveLedStatesBeforeStop()
        {
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
            led.Fill = fill;

            var textBlock = FindTextBlockForLed(led);
            if (textBlock != null)
            {
                textBlock.Foreground = foreground;
            }
        }

        private void UpdateVoyants()
        {
            voyantLed1.Fill = ellipseLed1.Fill == Brushes.Black ? Brushes.Black : Brushes.White;
            voyantLed2.Fill = ellipseLed2.Fill == Brushes.Black ? Brushes.Black : Brushes.Blue;
            voyantLed3.Fill = ellipseLed3.Fill == Brushes.Black ? Brushes.Black : Brushes.Orange;
        }

        private bool isStopBtnPressed = false;
        private void EllipseStopBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
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
                if (!isSimulation) serialPort1.Write(encodedMessage, 0, encodedMessage.Length);
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
                if (!isSimulation) serialPort1.Write(encodedMessage, 0, encodedMessage.Length);
            }
        }
        #endregion


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------- ONGLET 2
        #region ONGLET 2
        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
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
            if (!isLaptop && isSerialPortAvailable) serialPort1.Write(encodedMessage, 0, encodedMessage.Length);
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
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
            SendPIDParams();
        }

        private void SendPIDParams()
        {
            byte[] rawDataLin = UARTProtocol.UartEncode(new SerialCommandSetPID(Pid.PID_LIN, 3, 50, 0, 4, 4, 4));
            byte[] rawDataAng = UARTProtocol.UartEncode(new SerialCommandSetPID(Pid.PID_ANG, 4, 50, 0, 4, 4, 4));

            if (!isSimulation) serialPort1.Write(rawDataLin, 0, rawDataLin.Length);
            if (!isSimulation) serialPort1.Write(rawDataAng, 0, rawDataAng.Length);
        }

        private void SendConsignes()
        {
            float consVitesseLin = (float)upDownLin.Value;
            float consVitesseAng = (float)upDownAng.Value;

            byte[] rawDataConsLin = UARTProtocol.UartEncode(new SerialCommandSetconsigneLin(consVitesseLin));
            byte[] rawDataConsAng = UARTProtocol.UartEncode(new SerialCommandSetconsigneAng(consVitesseAng));

            if (!isSimulation) serialPort1.Write(rawDataConsLin, 0, rawDataConsLin.Length);
            if (!isSimulation) serialPort1.Write(rawDataConsAng, 0, rawDataConsAng.Length);
        }

        private void MoveIndicator(ToggleButton toggleButton, bool isChecked)
        {
            if (toggleButton.Template.FindName("toggleIndicator", toggleButton) is Ellipse toggleIndicator)
            {
                toggleIndicator.VerticalAlignment = isChecked ? VerticalAlignment.Top : VerticalAlignment.Bottom;
            }
        }

        private void SendPID_Click(object sender, RoutedEventArgs e)
        {
            SendPIDParams();
        }

        private void SendConsigne_Click(object sender, RoutedEventArgs e)
        {
            SendConsignes();
        }

        private void SendModeManu_Checked(object sender, RoutedEventArgs e)
        {
            robot.autoModeActivated = false;
            isHooking = true;

            byte[] rawDataModeManu = UARTProtocol.UartEncode(new SerialCommandSetRobotMode((byte)0));
            if (!isSimulation) serialPort1.Write(rawDataModeManu, 0, rawDataModeManu.Length);
        }

        private void SendModeManu_Unchecked(object sender, RoutedEventArgs e)
        {
            robot.autoModeActivated = true;
            isHooking = false;

            byte[] rawDataStateModeAuto = UARTProtocol.UartEncode(new SerialCommandSetRobotMode((byte)1));
            if (!isSimulation) serialPort1.Write(rawDataStateModeAuto, 0, rawDataStateModeAuto.Length);
        }

        public enum StateRobot
        {
            STATE_ATTENTE = 0,
            STATE_STOP = 12,
            STATE_AVANCE = 2,
            STATE_TOURNE_SUR_PLACE_GAUCHE = 8,
            STATE_TOURNE_SUR_PLACE_DROITE = 10,
            STATE_RECULE = 14,
        }

        private void GlobalHookKeyPress(object? sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            Debug.WriteLine(e.KeyChar);

            if (robot.autoModeActivated == false && isHooking)
            {

                if (e.KeyChar == '8')
                {
                    byte[] rawDataStateAvance = UARTProtocol.UartEncode(new SerialCommandSetRobotState((byte)StateRobot.STATE_AVANCE));
                    if (!isSimulation) serialPort1.Write(rawDataStateAvance, 0, rawDataStateAvance.Length);
                }
                else if (e.KeyChar == '4')
                {
                    byte[] rawDataStateGauche = UARTProtocol.UartEncode(new SerialCommandSetRobotState((byte)StateRobot.STATE_TOURNE_SUR_PLACE_GAUCHE));
                    if (!isSimulation) serialPort1.Write(rawDataStateGauche, 0, rawDataStateGauche.Length);
                }
                else if (e.KeyChar == '2')
                {
                    byte[] rawDataStateRecule = UARTProtocol.UartEncode(new SerialCommandSetRobotState((byte)StateRobot.STATE_RECULE));
                    if (!isSimulation) serialPort1.Write(rawDataStateRecule, 0, rawDataStateRecule.Length);
                }
                else if (e.KeyChar == '6')
                {
                    byte[] rawDataStateDroite = UARTProtocol.UartEncode(new SerialCommandSetRobotState((byte)StateRobot.STATE_TOURNE_SUR_PLACE_DROITE));
                    if (!isSimulation) serialPort1.Write(rawDataStateDroite, 0, rawDataStateDroite.Length);
                }
                else if (e.KeyChar == '5')
                {
                    byte[] rawDataStateAttente = UARTProtocol.UartEncode(new SerialCommandSetRobotState((byte)StateRobot.STATE_STOP));
                    if (!isSimulation) serialPort1.Write(rawDataStateAttente, 0, rawDataStateAttente.Length);
                }
            }
        }
        #endregion


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------- ONGLET 3
        #region ONGLET 3
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Image image && image.IsMouseCaptured)
            {
                Point position = e.GetPosition(image);
                bool shouldRepositionCursor = false;

                Point cursorPositionInScreen = image.PointToScreen(position);

                Point imageTopLeft = image.PointToScreen(new Point(0, 0));
                Point imageBottomRight = image.PointToScreen(new Point(image.ActualWidth, image.ActualHeight));

                // Ajuster la position du curseur pour qu'il reste dans les limites de l'image
                if (cursorPositionInScreen.X < imageTopLeft.X || cursorPositionInScreen.X > imageBottomRight.X ||
                    cursorPositionInScreen.Y < imageTopLeft.Y || cursorPositionInScreen.Y > imageBottomRight.Y)
                {
                    shouldRepositionCursor = true;
                    if (cursorPositionInScreen.X < imageTopLeft.X) cursorPositionInScreen.X = imageTopLeft.X;
                    if (cursorPositionInScreen.X > imageBottomRight.X) cursorPositionInScreen.X = imageBottomRight.X;
                    if (cursorPositionInScreen.Y < imageTopLeft.Y) cursorPositionInScreen.Y = imageTopLeft.Y;
                    if (cursorPositionInScreen.Y > imageBottomRight.Y) cursorPositionInScreen.Y = imageBottomRight.Y;
                }

                if (shouldRepositionCursor)
                {
                    SetCursorPos((int)cursorPositionInScreen.X, (int)cursorPositionInScreen.Y);
                }

                // Origine en bas à gauche
                double factorX = 300.0 / image.ActualWidth;
                double factorY = 200.0 / image.ActualHeight;
                double realX = position.X * factorX;
                double realY = (image.ActualHeight - position.Y) * factorY;

                targetPositionX.Text = Math.Max(0, realX).ToString("F0", CultureInfo.InvariantCulture);
                targetPositionY.Text = Math.Max(0, realY).ToString("F0", CultureInfo.InvariantCulture);

                robotInitX.Text = Math.Max(0, realX).ToString("F0", CultureInfo.InvariantCulture);
                robotInitY.Text = Math.Max(0, realY).ToString("F0", CultureInfo.InvariantCulture);
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image image)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    image.CaptureMouse();
                    SetGridsOpacity(0.2);
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    image.ReleaseMouseCapture();
                    SetGridsOpacity(0.75);
                }
            }
        }

        private void SetGridsOpacity(double opacity)
        {
            gridParcours.Opacity = opacity;
            gridOscillo.Opacity = opacity;
            gridFeedback.Opacity = opacity;
        }

        private void SendTargetXY_Click(object sender, RoutedEventArgs e)
        {
            UpdateTargetPosition();
            isWaypointSent = true;
        }

        private void UpdateTargetPosition()
        {
            if (float.TryParse(targetPositionX.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float targetX) &&
                float.TryParse(targetPositionY.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float targetY))
            {
                targetX = Math.Clamp(targetX, 0, 300);
                targetY = Math.Clamp(targetY, 0, 200);

                // SetRobotPosition(targetX, targetY, 0);
                SetTargetPosition(targetX, targetY);


                byte[] rawDataGhostXY = UARTProtocol.UartEncode(new SerialCommandSetGhostPosition(targetX / 100, targetY / 100));
                if (!isSimulation) serialPort1.Write(rawDataGhostXY, 0, rawDataGhostXY.Length);

                Debug.WriteLine($"\n    X: {targetX}, Y: {targetY}");
            }
        }

        private void SetRobotPosition(float x, float y, float theta)
        {
            double canvasX = (x / 300) * canvasTerrain.ActualWidth;
            double canvasY = canvasTerrain.ActualHeight - (y / 200) * canvasTerrain.ActualHeight;

            canvasX -= movingRobot.Width / 2;
            canvasY -= movingRobot.Height / 2;

            Canvas.SetLeft(movingRobot, canvasX);
            Canvas.SetTop(movingRobot, canvasY);

            RotateTransform rotateTransform = new RotateTransform(theta, movingRobot.Width / 2, movingRobot.Height / 2);
            movingRobot.RenderTransform = rotateTransform;

            movingRobot.Visibility = Visibility.Visible;
        }

        private void InitRobotButton_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(robotInitX.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float initX) &&
                float.TryParse(robotInitY.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float initY) &&
                float.TryParse(robotInitTheta.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float initTheta))
            {
                initX = Math.Clamp(initX, 0, 300);
                initY = Math.Clamp(initY, 0, 200);

                SetRobotPosition(initX, initY, initTheta);
            }
        }

        private void InitMovingRobotPosition() // Position visuelle par défaut
        {
            Canvas.SetLeft(movingRobot, 17);
            Canvas.SetTop(movingRobot, 411);
            movingRobot.Visibility = Visibility.Visible;
        }

        public void RobotGoTo(string pointName)
        {
            if (trajectoryManager.pointsList.TryGetValue(pointName, out var point))
            {
                targetPositionX.Text = point.X.ToString("F0", CultureInfo.InvariantCulture);
                targetPositionY.Text = point.Y.ToString("F0", CultureInfo.InvariantCulture);

                robotInitX.Text = point.X.ToString("F0", CultureInfo.InvariantCulture);
                robotInitY.Text = point.Y.ToString("F0", CultureInfo.InvariantCulture);

                // UpdateTargetPosition();
            }
        }

        #region Boutons Waypoints Zones
        private void BoutonCentre_Click(object sender, RoutedEventArgs e)
        {
            RobotGoTo("Centre");
        }

        private void BoutonZoneHautGauche_Click(object sender, RoutedEventArgs e)
        {
            RobotGoTo("Zone Haut Gauche");
        }

        private void BoutonZoneMilieuGauche_Click(object sender, RoutedEventArgs e)
        {
            RobotGoTo("Zone Milieu Gauche");
        }

        private void BoutonZoneBasGauche_Click(object sender, RoutedEventArgs e)
        {
            RobotGoTo("Zone Bas Gauche");
        }

        private void BoutonZoneHautDroit_Click(object sender, RoutedEventArgs e)
        {
            RobotGoTo("Zone Haut Droit");
        }

        private void BoutonZoneMilieuDroit_Click(object sender, RoutedEventArgs e)
        {
            RobotGoTo("Zone Milieu Droit");
        }

        private void BoutonZoneBasDroit_Click(object sender, RoutedEventArgs e)
        {
            RobotGoTo("Zone Bas Droit");
        }

        private void BoutonBaliseHautGauche_Click(object sender, RoutedEventArgs e)
        {
            RobotGoTo("Balise Haut Gauche");
        }

        private void BoutonBaliseBasGauche_Click(object sender, RoutedEventArgs e)
        {
            RobotGoTo("Balise Bas Gauche");
        }

        private void BoutonBaliseHautDroit_Click(object sender, RoutedEventArgs e)
        {
            RobotGoTo("Balise Haut Droit");
        }

        private void BoutonBaliseBasDroit_Click(object sender, RoutedEventArgs e)
        {
            RobotGoTo("Balise Bas Droit");
        }
        #endregion

        private void AddPointToRoute_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RunRoute_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearRoute_Click(object sender, RoutedEventArgs e)
        {
            textBoxParcours.Text = "";
        }

        public void UpdateFeedbackWaypoint()
        {
            var robotStatus = (distance: Math.Abs(robot.ghost.distanceToTarget), angle: Math.Abs(robot.ghost.angleToTarget));
            var currentMillis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (!isMoving && lastWaypointTimeMillis != 0 && (currentMillis - lastWaypointTimeMillis) < 3000)
            {
                return;
            }
            else if (!isMoving && lastWaypointTimeMillis != 0)
            {
                txtFeedbackWaypoint.Text += "\n-->  ATTENTE DE LA PROCHAINE CIBLE";
                lastWaypointTimeMillis = 0;
            }

            if (isWaypointSent)
            {
                if (string.IsNullOrWhiteSpace(targetPositionX.Text) || string.IsNullOrWhiteSpace(targetPositionY.Text))
                {
                    txtFeedbackWaypoint.Text += "\n-->  VEUILLEZ RENSEIGNER UNE CIBLE";
                    isWaypointSent = false;
                    lastWaypointTimeMillis = currentMillis;
                    return;
                }

                string targetXText = targetPositionX.Text;
                string targetYText = targetPositionY.Text;
                txtFeedbackWaypoint.Text += $"\n-->  X: {targetXText}, Y: {targetYText}";
                txtFeedbackWaypoint.Text += $"\n-->  WAYPOINT TRANSMIS";

                isWaypointSent = false;
                isMoving = true;
                lastWaypointTimeMillis = currentMillis;
                return;
            }

            switch (robotStatus)
            {
                case var status when status.angle >= 2:
                    txtFeedbackWaypoint.Text += "\n-->  ORIENTATION SUR LE WAYPOINT";
                    lastWaypointTimeMillis = 0;
                    break;

                case var status when status.angle <= 2 && status.distance >= 5:
                    txtFeedbackWaypoint.Text += "\n-->  DÉPLACEMENT JUSQU'AU WAYPOINT";
                    lastWaypointTimeMillis = 0;
                    break;

                case var status when status.angle <= 2 && status.distance <= 5 && isMoving:
                    txtFeedbackWaypoint.Text += "\n-->  WAYPOINT ATTEINT \u2705";
                    isMoving = false;
                    lastWaypointTimeMillis = currentMillis;
                    break;

                default:
                    txtFeedbackWaypoint.Text = "-->  ATTENTE DE LA PROCHAINE CIBLE";
                    break;
            }
        }


        public void SetTargetPosition(double targetX, double targetY)
        {
            trajectoryManager.Generator.GhostPosition.TargetX = targetX;
            trajectoryManager.Generator.GhostPosition.TargetY = targetY;

            trajectoryManager.Generator.GhostPosition.State = TrajectoryManager.TrajectoryState.Idle;
        }

        private DispatcherTimer trajectoryUpdateTimer;

        private void InitializeTrajectoryUpdateTimer()
        {
            trajectoryUpdateTimer = new DispatcherTimer();
            trajectoryUpdateTimer.Interval = TimeSpan.FromMilliseconds(20);
            trajectoryUpdateTimer.Tick += TrajectoryUpdateTimer_Tick;
            trajectoryUpdateTimer.Start();
        }

        private void TrajectoryUpdateTimer_Tick(object sender, EventArgs e)
        {
            trajectoryManager.Generator.UpdateTrajectory();
            UpdatemovingRobot();
        }

        private void UpdatemovingRobot()
        {
            var ghostPos = trajectoryManager.Generator.GhostPosition;

            double canvasWidth = canvasTerrain.ActualWidth;
            double canvasHeight = canvasTerrain.ActualHeight;
            double scaleX = canvasWidth / 300.0;
            double scaleY = canvasHeight / 200.0;

            double movingRobotCenterX = movingRobot.Width / 2.0;
            double movingRobotCenterY = movingRobot.Height / 2.0;

            double canvasX = (ghostPos.X * scaleX) - movingRobotCenterX;
            double canvasY = (canvasHeight - (ghostPos.Y * scaleY)) - movingRobotCenterY;

            Canvas.SetLeft(movingRobot, canvasX);
            Canvas.SetTop(movingRobot, canvasY);

            double rotationDegrees = ghostPos.Theta * (320.0 / Math.PI);

            RotateTransform rotateTransform = new RotateTransform(rotationDegrees, movingRobotCenterX, movingRobotCenterY);
            movingRobot.RenderTransform = rotateTransform;
        }

        private void SendPIDPos_Click(object sender, RoutedEventArgs e)
        {
            SendPIDPosParams();
        }

        private void SendPIDPosParams()
        {
            //byte[] rawDataPos = UARTProtocol.UartEncode(new SerialCommandSetPIDPosition(0, 0, 0, 0, 0, 0));
            //serialPort1.Write(rawDataPos, 0, rawDataPos.Length);
        }


        #endregion
    }
}