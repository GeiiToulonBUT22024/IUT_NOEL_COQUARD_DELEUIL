using ExtendedSerialPort;
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
using static robotInterface.TrajectoryManager;
using static System.Windows.Forms.AxHost;
using Constants;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
using System.ComponentModel;


namespace robotInterface
{
    public partial class MainWindow : Window
    {
        private readonly Robot robot = new();
        private readonly SerialProtocolManager UARTProtocol = new();
        private readonly TrajectoryManager trajectoryManager = new();

        private bool isSimulation = false;
        private readonly int defaultMode = ASSERV; // AUTO/ASSERV

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            InitializeSerialPort();
            InitializeKeylogger();
            InitializeLedStates();
            InitializeOscilloscopes();
            InitializeUI();
            InitializeRobotPosition(22, 22, 0);
        }


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------- INITIALISATIONS
        #region INITIALISATIONS

        private DispatcherTimer timerDisplay, timerMovingRobot;
        private ReliableSerialPort serialPort1;
        private bool isSerialPortAvailable = false;

        private void InitializeTimer()
        {
            timerDisplay = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100)
            };
            timerDisplay.Tick += TimerDisplay_Tick;
            timerDisplay.Start();

            timerMovingRobot = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(5)
            };
            timerMovingRobot.Tick += TimerMovingRobot_Tick;
            timerMovingRobot.Start();
        }

        private void InitializeSerialPort()
        {
            string comPort = "COM6";
            UARTProtocol.setRobot(robot);

            if (SerialPort.GetPortNames().Contains(comPort))
            {
                serialPort1 = new ReliableSerialPort(comPort, 115200, Parity.None, 8, StopBits.One);
                serialPort1.OnDataReceivedEvent += SerialPort1_DataReceived;
                try
                {
                    serialPort1.Open();
                    isSerialPortAvailable = true;
                }
                catch (System.IO.IOException ex) { Debug.WriteLine($"IOException lors de l'utilisation du port série: {ex.Message}"); }
                catch (Exception ex)
                {
                    MessageBox.Show("Impossible d'ouvrir le port série : " + ex.Message);
                    return;
                }
            }
            else
            {
                if (!isSimulation)
                {
                    MessageBoxResult result = MessageBox.Show("Le port COM n'est pas accessible, voulez-vous activer le mode Simulation ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    isSimulation = (result == MessageBoxResult.Yes);

                    if (result == MessageBoxResult.No)
                    {
                        MessageBox.Show("Veuillez changer le numéro de port COM ou activer le mode Simulation", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Shutdown();
                    }
                }
            }
        }

        private void InitializeUI()
        {
            DataContext = this;

            tabs.SelectedItem = tabSupervision;

            this.Loaded += (sender, e) => InitToggleSwitch(ToggleSwitch, null);
        }

        private void InitializeKeylogger()
        {
            IKeyboardMouseEvents m_GlobalHook;
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyPress += GlobalHookKeyPress;
        }

        private void InitializeRobotPosition(double x, double y, double thetaDeg)
        {
            var ghost = trajectoryManager.Generator.GhostPosition;
            double thetaRad = thetaDeg * Math.PI / 180.0;

            // Initialiser la position du robot et du ghost
            robot.ghost.x = robot.ghost.ghostPosX = (float)x;
            robot.ghost.y = robot.ghost.ghostPosY = (float)y;
            robot.ghost.theta = (float)thetaRad;

            // Initialiser la position dans le TrajectoryManager
            ghost.X = ghost.TargetX = x;
            ghost.Y = ghost.TargetY = y;
            ghost.Theta = thetaRad;

            // Positionner le robot sur l'image du terrain
            PositionRobotOnCanvas(x, y);
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

        private void InitializeOscilloscopes()
        {
            oscilloLinearSpeed.AddOrUpdateLine(1, 200, "");
            oscilloLinearSpeed.ChangeLineColor(1, Color.FromRgb(0, 255, 0));

            oscilloAngularSpeed.AddOrUpdateLine(1, 200, "");
            oscilloAngularSpeed.ChangeLineColor(1, Color.FromRgb(0, 0, 255));

            oscilloPos.AddOrUpdateLine(2, 200, "");
            oscilloPos.ChangeLineColor(2, Color.FromRgb(255, 0, 0));
        }
        #endregion


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------- UI SETTINGS
        #region UI SETTINGS

        #region Réception de l'UART
        public void SerialPort1_DataReceived(object? sender, DataReceivedArgs e)
        {
            for (int i = 0; i < e.Data.Length; i++)
            {
                UARTProtocol.DecodeMessage(e.Data[i]);
            }
        }
        #endregion

        #region Fonctions des touches
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close(); // Fermeture de l'application sur la touche Escape
            }
            else if (e.Key == Key.Tab)
            {
                e.Handled = true;
                tabs.SelectedIndex = (tabs.SelectedIndex + 1) % tabs.Items.Count; // Changement d'onglet sur la touche Tab
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Multiply) // Touche "*" sur le pavé numérique
                this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove(); // Permet de déplacer la fenêtre
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized; // Minimise la fenêtre
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e) // Fullscreen
        {
            this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Ferme la fenêtre
        }
        #endregion

        #region Affichage état simulation
        private string simulationText;
        public string SimulationStatus
        {
            get => isSimulation ? " -  Simulation Mode" : string.Empty;
            set
            {
                if (simulationText != value)
                {
                    simulationText = value;
                    OnPropertyChanged(nameof(SimulationStatus));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #endregion


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------- DISPLAY UPDATE
        #region DISPLAY UPDATE
        private void TimerDisplay_Tick(object? sender, EventArgs e)
        {
            // TextBox
            if (robot.receivedText != "") { textBoxReception.Text += robot.receivedText; robot.receivedText = ""; }
            while (robot.stringListReceived.Count != 0) textBoxReception.Text += robot.stringListReceived.Dequeue();

            // Valeurs QEI
            labelPositionXOdo.Content = "Position X\n{value} m".Replace("{value}", robot.positionXOdo.ToString("F3"));
            labelPositionYOdo.Content = "Position Y\n{value} m".Replace("{value}", robot.positionYOdo.ToString("F3"));
            labelAngle.Content = "Angle\n{value} rad".Replace("{value}", robot.angle.ToString("F2"));
            labelTimestamp.Content = "Timer\n{value} s".Replace("{value}", robot.timestamp.ToString("F1"));
            labelVitLin.Content = "Vitesse Linéaire\n{value} m/ms".Replace("{value}", robot.vitLin.ToString("F3"));
            labelVitAng.Content = "Vitesse Angulaire\n{value} rad/ms".Replace("{value}", robot.vitAng.ToString("F2"));

            // Oscilloscopes
            var ghost = trajectoryManager.Generator.GhostPosition;
            if (isSimulation) oscilloPos.AddPointToLine(2, ghost.X, ghost.Y);
            else oscilloPos.AddPointToLine(2, robot.positionXOdo, robot.positionYOdo);
            oscilloAngularSpeed.AddPointToLine(1, robot.timestamp, robot.vitAng);
            oscilloLinearSpeed.AddPointToLine(1, robot.timestamp, robot.vitLin);

            // Tableau asservissement vitesse
            asservSpeedDisplay.UpdatePolarSpeedCorrectionGains(robot.pidLin.Kp, robot.pidAng.Kp, robot.pidLin.Ki, robot.pidAng.Ki, robot.pidLin.Kd, robot.pidAng.Kd);
            asservSpeedDisplay.UpdatePolarSpeedCorrectionLimits(robot.pidLin.erreurPmax, robot.pidAng.erreurPmax, robot.pidLin.erreurImax, robot.pidAng.erreurImax, robot.pidLin.erreurDmax, robot.pidAng.erreurDmax);
            asservSpeedDisplay.UpdatePolarSpeedCommandValues(robot.pidLin.cmdLin, robot.pidAng.cmdAng);
            asservSpeedDisplay.UpdatePolarSpeedConsigneValues(robot.pidLin.consigne, robot.pidAng.consigne);
            asservSpeedDisplay.UpdatePolarSpeedCorrectionValues(robot.pidLin.corrP, robot.pidAng.corrP, robot.pidLin.corrI, robot.pidAng.corrI, robot.pidLin.corrD, robot.pidAng.corrD);
            asservSpeedDisplay.UpdatePolarSpeedErrorValues(robot.pidLin.erreur, robot.pidAng.erreur);
            asservSpeedDisplay.UpdatePolarOdometrySpeed(robot.vitLin, robot.vitAng);
            asservSpeedDisplay.UpdateDisplay();

            // Tableau asservissement position -> Ki et les paramètres angulaires ne sont pas utilisés dans ce correcteur PD, donc fixés à 0
            asservSpeedDisplayPosition.UpdatePolarSpeedCorrectionGains(robot.pidPos.Kp, 0, robot.pidPos.Ki, 0, robot.pidPos.Kd, 0);
            asservSpeedDisplayPosition.UpdatePolarSpeedCorrectionLimits(robot.pidPos.erreurPmax, 0, robot.pidPos.erreurImax, 0, robot.pidPos.erreurDmax, 0);
            asservSpeedDisplayPosition.UpdatePolarSpeedCommandValues(robot.pidPos.cmdLin, 0);
            asservSpeedDisplayPosition.UpdatePolarSpeedConsigneValues(robot.pidPos.consigne, 0);
            asservSpeedDisplayPosition.UpdatePolarSpeedCorrectionValues(robot.pidPos.corrP, 0, robot.pidPos.corrI, 0, robot.pidPos.corrD, 0);
            asservSpeedDisplayPosition.UpdatePolarSpeedErrorValues(robot.pidPos.erreur, 0);
            asservSpeedDisplayPosition.UpdatePolarOdometrySpeed(robot.vitLin, robot.vitAng);
            asservSpeedDisplayPosition.UpdateDisplay();

            // Feedback positionnement
            if (isSimulation)
            {
                double AngleToTargetDeg = (ghost.Theta - ghost.AngleToTarget) * (180.0 / Math.PI);

                if (ghost.DistanceToTarget == 0.00 && AngleToTargetDeg != 0.00) labelAngleToTarget.Content = "Angle à la cible : 0 °";
                else labelAngleToTarget.Content = "Angle à la cible : {value} °".Replace("{value}", AngleToTargetDeg.ToString("F0"));

                labelDistanceToTarget.Content = "Distance à la cible : {value} cm".Replace("{value}", ghost.DistanceToTarget.ToString("F0"));
                labelGhostPosX.Content = "Cible X : {value} ".Replace("{value}", ghost.TargetX.ToString("F0"));
                labelGhostPosY.Content = "Cible Y : {value} ".Replace("{value}", ghost.TargetY.ToString("F0"));
                labelOdoPosX.Content = "Robot X : {value} ".Replace("{value}", ghost.X.ToString("F0"));
                labelOdoPosY.Content = "Robot Y : {value} ".Replace("{value}", ghost.Y.ToString("F0"));
            }
            else
            {
                labelDistanceToTarget.Content = "Distance à la cible : {value} m".Replace("{value}", robot.ghost.distanceToTarget.ToString("F2"));
                labelAngleToTarget.Content = "Angle à la cible : {value} rad".Replace("{value}", robot.ghost.angleToTarget.ToString("F2"));
                labelGhostPosX.Content = "Ghost X : {value} ".Replace("{value}", robot.ghost.ghostPosX.ToString("F2"));
                labelGhostPosY.Content = "Ghost Y : {value} ".Replace("{value}", robot.ghost.ghostPosY.ToString("F2"));
                labelOdoPosX.Content = "Odo X : {value} ".Replace("{value}", robot.positionXOdo.ToString("F2"));
                labelOdoPosY.Content = "Odo Y : {value} ".Replace("{value}", robot.positionYOdo.ToString("F2"));
            }

            // Graphiques visualisation
            UpdateTelemetreGauges();
            UpdateTelemetreBoxes();
            UpdateSpeedGauges();

        }
        #endregion


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------- ONGLET 1
        #region ONGLET 1 

        #region Envoi de messages
        private bool SendMessage(bool _)
        {
            if (textBoxEmission.Text == "\r\n" || textBoxEmission.Text == "") return false;

            byte[] payload = new byte[textBoxEmission.Text.Length];

            for (int i = 0; i < textBoxEmission.Text.Length; i++)
                payload[i] = (byte)textBoxEmission.Text[i];

            serialPort1.SendMessage(this, payload);
            textBoxEmission.Text = "";
            return true;
        }

        private void BtnEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(false);
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            textBoxReception.Text = "";
        }

        private void TextBoxEmission_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!SendMessage(true))
                {
                    textBoxEmission.Text = "";
                }
            }
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            byte[] byteList = new byte[20];
            for (int i = 19; i >= 0; i--)
                byteList[i] = (byte)(2 * i);

            if (!isSimulation) serialPort1.Write(byteList, 0, 20);
        }
        #endregion

        #region Obstacles Box Télémètres
        private void UpdateTelemetreBoxes() // Gestion de l'affichage des obsctacles par rapport au robot
        {
            var scaleCoef = 2; // Adapte la taille de la translation

            var angle = -47.703;
            TransformGroup customTGELeft = new();
            customTGELeft.Children.Add(new RotateTransform(angle));
            Vector2 translationVector = GetBTTranslationVector(angle, scaleCoef, robot.distanceTelemetreMelenchon);
            customTGELeft.Children.Add(new TranslateTransform(translationVector.X, translationVector.Y));

            boxTeleELeft.RenderTransform = customTGELeft;

            // -------------------------------------------------

            angle = -24.786;
            TransformGroup customTGLeft = new();
            customTGLeft.Children.Add(new RotateTransform(angle));
            translationVector = GetBTTranslationVector(angle, scaleCoef, robot.distanceTelemetreGauche);
            customTGLeft.Children.Add(new TranslateTransform(translationVector.X, translationVector.Y));

            boxTeleLeft.RenderTransform = customTGLeft;

            // -------------------------------------------------

            angle = 0;
            TransformGroup customTGCenter = new();
            translationVector = GetBTTranslationVector(angle, scaleCoef, robot.distanceTelemetreCentre);
            customTGCenter.Children.Add(new TranslateTransform(translationVector.X, translationVector.Y));

            boxTeleCenter.RenderTransform = customTGCenter;

            // -------------------------------------------------

            angle = 24.786;

            TransformGroup customTGRight = new();
            customTGRight.Children.Add(new RotateTransform(angle));
            translationVector = GetBTTranslationVector(angle, scaleCoef, robot.distanceTelemetreDroit);
            customTGRight.Children.Add(new TranslateTransform(translationVector.X, translationVector.Y));

            boxTeleRight.RenderTransform = customTGRight;

            // -------------------------------------------------

            angle = 47.703;

            TransformGroup customTGERight = new();
            customTGERight.Children.Add(new RotateTransform(angle));
            translationVector = GetBTTranslationVector(angle, scaleCoef, robot.distanceTelemetreLePen);
            customTGERight.Children.Add(new TranslateTransform(translationVector.X, translationVector.Y));

            boxTeleERight.RenderTransform = customTGERight;
        }

        // Calculer les transformations des rectangles (obstacles)
        private static Vector2 GetBTTranslationVector(double angle, double scaleCoef, double distance)
        {
            Vector2 translateVector = new()
            {
                X = (float)(Math.Cos((Math.PI / 180) * (270 + angle)) * distance * scaleCoef),
                Y = (float)(Math.Sin((Math.PI / 180) * (270 + angle)) * distance * scaleCoef)
            };
            return translateVector;
        }
        #endregion

        #region Jauges distance Télémètres
        private void UpdateSpeedGauges()
        {
            UpdateGaugePointer(LeftGauge, robot.consigneGauche);
            UpdateGaugePointer(RightGauge, robot.consigneDroite);
        }

        // Gestion des jauges pour les vitesses
        private static void UpdateGaugePointer(SfCircularGauge gauge, double value)
        {
            if (gauge.Scales[0].Pointers.Count == 0)
            {
                CircularPointer circularPointer = new()
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

        private static Brush ChooseColor(float distance)
        {
            return distance < 20 ? Brushes.Red :       // Rouge pour les valeurs inférieures à 20
                   distance < 40 ? Brushes.Orange :    // Orange pour les valeurs entre 20 et 40
                   Brushes.Green;                      // Vert pour les valeurs supérieures à 40
        }
        #endregion

        #region Gestion des leds
        private void ToggleLed(Ellipse ellipse, byte numeroLed, bool isLedOn) // Logique de gestion des couleurs des LEDs (ON/OFF)
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

        private static TextBlock? FindTextBlockForLed(Ellipse ellipse)
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

        private SolidColorBrush led1FillBeforeStop;
        private SolidColorBrush led2FillBeforeStop;
        private SolidColorBrush led3FillBeforeStop;
        private SolidColorBrush led1ForegroundBeforeStop;
        private SolidColorBrush led2ForegroundBeforeStop;
        private SolidColorBrush led3ForegroundBeforeStop;

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

        private static void SetLedState(Ellipse led, SolidColorBrush fill, SolidColorBrush foreground)
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

        // Lors d'un clic, bascule l'état des leds
        private void EllipseLed_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse ellipse)
            {
                byte numeroLed = Convert.ToByte(ellipse.Tag);
                bool isLedOn = ellipse.Fill.ToString() == Brushes.Black.ToString();

                ToggleLed(ellipse, numeroLed, !isLedOn);
            }
        }

        // Clic sur l'ARU
        private bool isStopBtnPressed = false;
        private void EllipseStopBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isStopBtnPressed = !isStopBtnPressed;

            if (isStopBtnPressed)
            {
                SaveLedStatesBeforeStop();
                TurnOffAllLeds();

                tabs.Background = new SolidColorBrush(Color.FromArgb(50, 255, 20, 20)); // red

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

                tabs.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0d0d0d"));

                scaleTransform.ScaleX = 1.0;
                scaleTransform.ScaleY = 1.0;
                shadowEffect.ShadowDepth = 5;
                shadowEffect.BlurRadius = 10;

                var encodedMessage = UARTProtocol.UartEncode(new SerialCommandText("GO"));
                if (!isSimulation) serialPort1.Write(encodedMessage, 0, encodedMessage.Length);
            }
        }
        #endregion

        #endregion


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------- ONGLET 2
        #region ONGLET 2

        #region Mode asserv

        private const int AUTO = 0, ASSERV = 1;

        // Initialise et transmet le mode de fonctionnement du robot par défaut (Auto/Asserv)
        private void InitToggleSwitch(object sender, RoutedEventArgs? e)
        {
            if (sender is ToggleButton toggleButton)
            {
                toggleButton.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (toggleButton.Template?.FindName("toggleIndicator", toggleButton) is Ellipse)
                    {
                        var eventArgs = e ?? new RoutedEventArgs();
                        if (defaultMode == ASSERV) ToggleSwitch_Unchecked(toggleButton, eventArgs);
                        else if (defaultMode == AUTO) ToggleSwitch_Checked(toggleButton, eventArgs);
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

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
            if (!isSimulation && isSerialPortAvailable) serialPort1.Write(encodedMessage, 0, encodedMessage.Length);
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

            var encodedMessage = UARTProtocol.UartEncode(new SerialCommandText("asservEnabled"));
            if (!isSimulation && isSerialPortAvailable) serialPort1.Write(encodedMessage, 0, encodedMessage.Length);

            SendPIDParams();
            SendPIDPosParams();
        }

        // Inverse la position du rond blanc
        private static void MoveIndicator(ToggleButton toggleButton, bool isChecked)
        {
            toggleButton.Dispatcher.BeginInvoke(new Action(() =>
            {
                toggleButton.ApplyTemplate();
                if (toggleButton.Template?.FindName("toggleIndicator", toggleButton) is Ellipse toggleIndicator)
                    toggleIndicator.VerticalAlignment = isChecked ? VerticalAlignment.Top : VerticalAlignment.Bottom;
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        #endregion

        #region Consignes et PID
        private void SendPIDParams()
        {
            //byte[] rawDataLin = UARTProtocol.UartEncode(new SerialCommandSetPID(Pid.PID_LIN, 3, 50, 0, 4, 4, 4));
            byte[] rawDataLin = UARTProtocol.UartEncode(new SerialCommandSetPID(Pid.PID_LIN, 1.5f, 30, 0, 4, 4, 4));
            byte[] rawDataAng = UARTProtocol.UartEncode(new SerialCommandSetPID(Pid.PID_ANG, 1.5f, 20, 0, 10, 10, 10));
            if (!isSimulation) serialPort1.Write(rawDataLin, 0, rawDataLin.Length);
            if (!isSimulation) serialPort1.Write(rawDataAng, 0, rawDataAng.Length);
        }

        private void SendPIDPosParams()
        {
            byte[] rawDataPos = UARTProtocol.UartEncode(new SerialCommandSetPID(Pid.PID_POS, 0, 0, 0, 4, 4, 4));
            if (!isSimulation) serialPort1.Write(rawDataPos, 0, rawDataPos.Length);
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

        private void SendPID_Click(object sender, RoutedEventArgs e)
        {
            SendPIDParams();
        }

        private void SendPIDPos_Click(object sender, RoutedEventArgs e)
        {
            SendPIDPosParams();
        }

        private void SendConsigne_Click(object sender, RoutedEventArgs e)
        {
            SendConsignes();
        }
        #endregion

        #region Keylogger
        public enum StateRobot
        {
            STATE_ATTENTE = 0,
            STATE_STOP = 12,
            STATE_AVANCE = 2,
            STATE_TOURNE_SUR_PLACE_GAUCHE = 8,
            STATE_TOURNE_SUR_PLACE_DROITE = 10,
            STATE_RECULE = 14,
        }

        private bool isHooking = false;

        private void GlobalHookKeyPress(object? sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            //Debug.WriteLine(e.KeyChar);

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

        #region Mode manuel
        private void SendModeManu_Checked(object sender, RoutedEventArgs e)
        {
            robot.autoModeActivated = false;
            isHooking = true;

            byte[] rawDataModeManu = UARTProtocol.UartEncode(new SerialCommandSetRobotMode((byte)Robot.MODE_MANUEL));
            if (!isSimulation) serialPort1.Write(rawDataModeManu, 0, rawDataModeManu.Length);
        }

        private void SendModeManu_Unchecked(object sender, RoutedEventArgs e)
        {
            robot.autoModeActivated = true;
            isHooking = false;

            byte[] rawDataStateModeAuto = UARTProtocol.UartEncode(new SerialCommandSetRobotMode((byte)Robot.MODE_AUTO));
            if (!isSimulation) serialPort1.Write(rawDataStateModeAuto, 0, rawDataStateModeAuto.Length);
        }
        #endregion

        #endregion


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------- ONGLET 3
        #region ONGLET 3

        #region Gestion du curseur sur la carte
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Image image && image.IsMouseCaptured)
            {
                bool shouldRepositionCursor = false;
                Point position = e.GetPosition(image);
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

                [DllImport("user32.dll")] static extern bool SetCursorPos(int X, int Y);

                if (shouldRepositionCursor)
                {
                    SetCursorPos((int)cursorPositionInScreen.X, (int)cursorPositionInScreen.Y);
                }

                // Convertir les coordonnées du canvas (origine en haut à gauche)
                double factorX = 300.0 / image.ActualWidth;
                double factorY = 200.0 / image.ActualHeight;
                double realX = position.X * factorX;
                double realY = (200.0 - position.Y * factorY);

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
                    image.Cursor = Cursors.Cross;
                    SetGridsOpacity(0.2);
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    image.ReleaseMouseCapture();
                    image.Cursor = Cursors.Hand;
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
        #endregion

        #region Logique d'affichage du robot
        private void InitRobotButton_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(robotInitX.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float initX) &&
                float.TryParse(robotInitY.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float initY) &&
                float.TryParse(robotInitTheta.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float initThetaDeg))
            {
                initX = Math.Clamp(initX, 0, 300);
                initY = Math.Clamp(initY, 0, 200);

                InitializeRobotPosition(initX, initY, initThetaDeg);
            }
        }

        // Positionnement de l'image du robot
        private void PositionRobotOnCanvas(double x, double y)
        {
            // Mise à l'échelle et positionnement du robot
            double scaleX = canvasTerrain.ActualWidth / 300.0;
            double scaleY = canvasTerrain.ActualHeight / 200.0;
            double movingRobotCenterX = movingRobot.Width / 2.0;
            double movingRobotCenterY = movingRobot.Height / 2.0;

            double convertedY = 200.0 - y;
            double canvasX = x * scaleX - movingRobotCenterX;
            double canvasY = convertedY * scaleY - movingRobotCenterY;

            Canvas.SetLeft(movingRobot, canvasX);
            Canvas.SetTop(movingRobot, canvasY);
        }

        private bool startSequence = false;
        private readonly List<Point> waypoints = new();

        // Mise à jour de la carte et du positionnement
        private void TimerMovingRobot_Tick(object? sender, EventArgs e)
        {
            trajectoryManager.Generator.UpdateTrajectory();
            UpdateMovingRobot();
            UpdateMovingState();

            if (startSequence)
            {
                double distanceToTarget = isSimulation ? trajectoryManager.Generator.GhostPosition.DistanceToTarget : robot.ghost.distanceToTarget;

                if (distanceToTarget == 0.00)
                    if (waypoints.Count > 0) StartNextWaypoint();
            }
        }

        // Mise à jour de la position et rotation de l'image du robot
        private void UpdateMovingRobot()
        {
            var ghost = trajectoryManager.Generator.GhostPosition;

            double scaleX = canvasTerrain.ActualWidth / 300.0;
            double scaleY = canvasTerrain.ActualHeight / 200.0;

            double movingRobotCenterX = movingRobot.Width / 2.0;
            double movingRobotCenterY = movingRobot.Height / 2.0;

            double ghostCenterX = movingGhost.Width / 2.0;
            double ghostCenterY = movingGhost.Height / 2.0;

            double convertedX, convertedY, rotationDegrees;
            double convertedGhostX, convertedGhostY, ghostRotationDegrees;

            if (isSimulation)
            {
                // Position et rotation pour le robot en simulation
                convertedX = ghost.X * scaleX;
                convertedY = (200.0 - ghost.Y) * scaleY;
                rotationDegrees = -ghost.Theta * (180.0 / Math.PI);

                movingGhost.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Position et rotation pour le robot réel
                convertedX = robot.ghost.x * scaleX;
                convertedY = (200.0 - robot.ghost.y) * scaleY;
                rotationDegrees = -robot.ghost.theta * (180.0 / Math.PI);

                // Position et rotation pour le ghost du robot réel
                convertedGhostX = robot.ghost.ghostPosX * scaleX;
                convertedGhostY = (200.0 - robot.ghost.ghostPosY) * scaleY;
                ghostRotationDegrees = -robot.ghost.theta * (180.0 / Math.PI);

                // Mise à jour de la position et rotation de l'image du ghost
                Canvas.SetLeft(movingGhost, convertedGhostX - ghostCenterX);
                Canvas.SetTop(movingGhost, convertedGhostY - ghostCenterY);

                RotateTransform ghostRotateTransform = new(ghostRotationDegrees, ghostCenterX, ghostCenterY);
                movingGhost.RenderTransform = ghostRotateTransform;
                movingGhost.Visibility = Visibility.Visible;
            }

            Canvas.SetLeft(movingRobot, convertedX - movingRobotCenterX);
            Canvas.SetTop(movingRobot, convertedY - movingRobotCenterY);

            RotateTransform rotateTransform = new(rotationDegrees, movingRobotCenterX, movingRobotCenterY);
            movingRobot.RenderTransform = rotateTransform;
            movingRobot.Visibility = Visibility.Visible;
        }
        #endregion

        #region Envoi des waypoints
        private void SendTargetXY_Click(object sender, RoutedEventArgs e)
        {
            SendTargetPosition();
        }

        private void SendTargetPosition()
        {
            if (float.TryParse(targetPositionX.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float targetX) &&
                float.TryParse(targetPositionY.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float targetY))
            {
                targetX = Math.Clamp(targetX, 0, (float)300.0);
                targetY = Math.Clamp(targetY, 0, (float)200.0);

                byte[] rawDataGhostXY = UARTProtocol.UartEncode(new SerialCommandSetGhostPosition(targetX / 100, targetY / 100)); // en mètres
                if (!isSimulation) serialPort1.Write(rawDataGhostXY, 0, rawDataGhostXY.Length);
                else
                {
                    // Transmission du waypoint au simulateur
                    trajectoryManager.Generator.GhostPosition.TargetX = targetX;
                    trajectoryManager.Generator.GhostPosition.TargetY = targetY;
                }

                waypointSent = true;

                Debug.WriteLine($"\n    X: {targetX}, Y: {targetY}\n");
            }
        }

        private void SetTargetPosition(float x, float y)
        {
            targetPositionX.Text = Math.Max(0, x).ToString("F0", CultureInfo.InvariantCulture);
            targetPositionY.Text = Math.Max(0, y).ToString("F0", CultureInfo.InvariantCulture);
            robotInitX.Text = Math.Max(0, x).ToString("F0", CultureInfo.InvariantCulture);
            robotInitY.Text = Math.Max(0, y).ToString("F0", CultureInfo.InvariantCulture);
        }

        #region Boutons Waypoints Zones
        private void BoutonCentre_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPosition(150, 100);
        }

        private void BoutonBaliseHautGauche_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPosition(75, 150);
        }

        private void BoutonBaliseBasGauche_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPosition(75, 50);
        }

        private void BoutonBaliseHautDroit_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPosition(225, 150);
        }

        private void BoutonBaliseBasDroit_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPosition(225, 50);
        }

        private void BoutonZoneHautGauche_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPosition(22, 178);
        }

        private void BoutonZoneMilieuGauche_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPosition(22, 100);
        }

        private void BoutonZoneBasGauche_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPosition(22, 22);
        }

        private void BoutonZoneHautDroit_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPosition(278, 178);
        }

        private void BoutonZoneMilieuDroit_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPosition(278, 100);
        }

        private void BoutonZoneBasDroit_Click(object sender, RoutedEventArgs e)
        {
            SetTargetPosition(278, 22);
        }
        #endregion

        #endregion

        #region Gestion du parcours
        // Lien entre les zones et leurs coordonnées pour l'affichage
        private readonly Dictionary<(float, float), string> knownZones = new()
        {
            {(150, 100), "Centre"},
            {(75, 150), "Balise Haut Gauche"},
            {(75, 50), "Balise Bas Gauche"},
            {(225, 150), "Balise Haut Droit"},
            {(225, 50), "Balise Bas Droit"},
            {(22, 178), "Zone Haut Gauche"},
            {(22, 100), "Zone Milieu Gauche"},
            {(22, 22), "Zone Bas Gauche"},
            {(278, 178), "Zone Haut Droit"},
            {(278, 100), "Zone Milieu Droit"},
            {(278, 22), "Zone Bas Droit"}
        };

        private void AddPointToSequence_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(targetPositionX.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float targetX) &&
                float.TryParse(targetPositionY.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float targetY))
            {
                targetX = Math.Clamp(targetX, 0, (float)300.0);
                targetY = Math.Clamp(targetY, 0, (float)200.0);

                waypoints.Add(new Point(targetX, targetY));

                string zoneName = knownZones.ContainsKey((targetX, targetY)) ? $" -  {knownZones[(targetX, targetY)]}" : string.Empty;
                textBoxParcours.Text += $"X: {targetX}, Y: {targetY} {zoneName}\n";
            }
        }

        private void RunSequence_Click(object sender, RoutedEventArgs e)
        {
            if (waypoints.Count > 0)
            {
                startSequence = true;
                StartNextWaypoint();
            }

            startingRoute = true;
        }

        private bool firstWaypointSent = false;
        private void StartNextWaypoint()
        {
            if (waypoints.Count > 0)
            {
                var nextWaypoint = waypoints[0];

                float nextWaypointX = (float)nextWaypoint.X;
                float nextWaypointY = (float)nextWaypoint.Y;

                byte[] rawDataGhostXY = UARTProtocol.UartEncode(new SerialCommandSetGhostPosition(nextWaypointX / 100, nextWaypointY / 100)); // en mètres
                if (!isSimulation) serialPort1.Write(rawDataGhostXY, 0, rawDataGhostXY.Length);
                else
                {
                    // Transmission du waypoint au simulateur
                    trajectoryManager.Generator.GhostPosition.TargetX = nextWaypointX;
                    trajectoryManager.Generator.GhostPosition.TargetY = nextWaypointY;
                }

                waypointSent = true;
                ClearMovingFeedback();

                if (firstWaypointSent) // Supprime le waypoint dans le feedback parcours
                {
                    var lines = textBoxParcours.Text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (lines.Count > 0)
                    {
                        lines.RemoveAt(0);
                        textBoxParcours.Text = string.Join("\n", lines) + "\n";
                    }
                }
                else firstWaypointSent = true;

                waypoints.RemoveAt(0);

                Debug.WriteLine($"\n    X: {nextWaypointX}, Y: {nextWaypointY}\n");
            }
        }

        private void ClearSequence_Click(object sender, RoutedEventArgs e)
        {
            ClearSequence();
        }

        private void ClearSequence()
        {
            textBoxParcours.Text = "";
            waypoints.Clear();
            startSequence = false;
        }
        #endregion

        #region Feedback de déplacement
        public enum MovingState
        {
            WaitingForWaypoint,
            WaypointSent,
            StartingRoute,
            OrientingToTarget,
            AlignedToTarget,
            MovingToTarget,
            TargetReached,
        }

        private MovingState currentMovingState = MovingState.WaitingForWaypoint;
        private DateTime lastTargetReachedTime;
        private string lastFeedbackMessage = string.Empty;
        private bool isAligned = false, targetReached = true, startingRoute = false, waypointSent = false;

        // Déterminer l'état de déplacement actuel du robot
        private void UpdateMovingState()
        {
            double distanceToTarget, angleToTarget;

            if (isSimulation)
            {
                var ghost = trajectoryManager.Generator.GhostPosition;
                distanceToTarget = ghost.DistanceToTarget;
                angleToTarget = Math.Abs(ghost.AngleToTarget - ghost.Theta);
            }
            else
            {
                distanceToTarget = robot.ghost.distanceToTarget;
                angleToTarget = robot.ghost.angleToTarget;
            }

            if (waypointSent)
            {
                currentMovingState = MovingState.WaypointSent;
                waypointSent = false;
                ClearMovingFeedback();

                feedbackMovingState.BorderBrush = new SolidColorBrush(Colors.SlateGray);
                feedbackMovingState.Background = new SolidColorBrush(Colors.Transparent);
            }
            else if (startingRoute)
            {
                currentMovingState = MovingState.StartingRoute;
                startingRoute = false;
            }
            else if (waypoints.Count == 0 && distanceToTarget == 0 && targetReached)
            {
                if ((DateTime.Now - lastTargetReachedTime).TotalSeconds >= 2)
                {
                    currentMovingState = MovingState.WaitingForWaypoint;
                    ClearMovingFeedback();
                    ClearSequence();

                    feedbackMovingState.BorderBrush = new SolidColorBrush(Colors.SlateGray);
                    feedbackMovingState.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
            else if (distanceToTarget > 0.0 && angleToTarget > 0.1)
            {
                currentMovingState = MovingState.OrientingToTarget;
                isAligned = false;
            }
            else if (distanceToTarget > 0.0 && angleToTarget <= 0.1 && !isAligned)
            {
                currentMovingState = MovingState.AlignedToTarget;
                isAligned = true;
            }
            else if (distanceToTarget > 0.0 && isAligned)
            {
                currentMovingState = MovingState.MovingToTarget;
                targetReached = false;
            }
            else if (distanceToTarget == 0 && !targetReached)
            {
                currentMovingState = MovingState.TargetReached;
                targetReached = true;
                lastTargetReachedTime = DateTime.Now;

                feedbackMovingState.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 100, 0)); // Background vert
                feedbackMovingState.Background = new SolidColorBrush(Color.FromArgb(50, 144, 238, 144));
            }

            UpdateMovingFeedback();
        }

        // Afficher le message selon l'état du robot
        private void UpdateMovingFeedback()
        {
            string feedbackMessage = currentMovingState switch
            {
                MovingState.WaitingForWaypoint => "→ ATTENTE DE LA PROCHAINE CIBLE",
                MovingState.WaypointSent => "→ WAYPOINT TRANSMIS",
                MovingState.StartingRoute => "→ DÉBUT DU PARCOURS",
                MovingState.OrientingToTarget => "→ ORIENTATION VERS LA CIBLE",
                MovingState.AlignedToTarget => "→ ROBOT ALIGNÉ",
                MovingState.MovingToTarget => "→ DÉPLACEMENT VERS LA CIBLE",
                MovingState.TargetReached => "→ WAYPOINT ATTEINT",
                _ => "",
            };

            if (feedbackMessage != lastFeedbackMessage)
            {
                feedbackMovingState.Text += feedbackMessage + Environment.NewLine;
                lastFeedbackMessage = feedbackMessage;
            }
        }

        private void ClearMovingFeedback()
        {
            feedbackMovingState.Text = string.Empty;
            lastFeedbackMessage = string.Empty;
        }
        #endregion

        #endregion

    }
}