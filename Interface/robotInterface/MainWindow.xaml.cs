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


        private bool isLaptop = false;

#pragma warning disable CS8618 
        public MainWindow()
#pragma warning restore CS8618
        {
            InitializeComponent();
            InitializeSerialPort();
            InitializeLedStates();

            timerDisplay = new DispatcherTimer();
            timerDisplay.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerDisplay.Tick += TimerDisplay_Tick;
            timerDisplay.Start();

            UARTProtocol.setRobot(robot);
            this.WindowStyle = WindowStyle.None;         // Désactive les styles Windows par défaut
            this.ResizeMode = ResizeMode.NoResize;       // Désactive le recadrage automatique de la fenêtre
            this.WindowState = WindowState.Maximized;    // Ouvre la fenêtre en plein écran automatiquement

            tabs.SelectedItem = tabAsservissement;       // Ouvrir l'onglet Asservissement au chargement

            ToggleSwitch.IsChecked = true;

            oscilloSpeed.AddOrUpdateLine(1, 200, "Ligne 1");
            oscilloSpeed.ChangeLineColor(1, Color.FromRgb(0, 255, 0));

            oscilloPos.AddOrUpdateLine(2, 200, "Ligne 2");
            oscilloPos.ChangeLineColor(2, Color.FromRgb(0, 0, 255));

            IKeyboardMouseEvents m_GlobalHook;

            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyPress += GlobalHookKeyPress;

            DataObject.AddPastingHandler(speedLinearUpDown, OnPaste);
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
                textBoxReception.Text += robot.stringListReceived.Dequeue();
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
                    //if (!isLaptop) isLaptop = false;
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
                if (!isLaptop)
                {
                    MessageBoxResult result = MessageBox.Show("Le port COM n'existe pas, travaillez-vous sur un PC portable ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    isLaptop = (result == MessageBoxResult.Yes);

                    if (result == MessageBoxResult.No)
                    {
                        MessageBox.Show("Veuillez changer le numéro de port.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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

            ApplyCanvasConfiguration(isLaptop);
        }

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

        private bool sendMessage(bool key)
        {
            if (textBoxEmission.Text == "\r\n" || textBoxEmission.Text == "") return false;

            byte[] payload = new byte[textBoxEmission.Text.Length];

            for (int i = 0; i < textBoxEmission.Text.Length; i++)
                payload[i] = (byte)textBoxEmission.Text[i];

            if (!isLaptop) serialPort1.SendMessage(this, payload);
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
            if ((e.Key == Key.Multiply) || // Touche "*" sur le pavé numérique sans Shift
                (e.KeyboardDevice.Modifiers == ModifierKeys.Shift && e.Key == Key.D8)) // Shift + 8 pour les claviers sans pavé numérique
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

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {

            byte[] byteList = new byte[20];
            for (int i = 19; i >= 0; i--)
                byteList[i] = (byte)(2 * i);

            if (!isLaptop) serialPort1.Write(byteList, 0, 20);
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
                if (!isLaptop) serialPort1.Write(rawData, 0, rawData.Length);
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
                if (!isLaptop) serialPort1.Write(encodedMessage, 0, encodedMessage.Length);
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
                if (!isLaptop) serialPort1.Write(encodedMessage, 0, encodedMessage.Length);
            }
        }

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

            if (isSerialPortAvailable)
            {
                var encodedMessage = UARTProtocol.UartEncode(new SerialCommandText("asservDisabled"));
                //if (!isLaptop && isSerialPortAvailable) serialPort1.Write(encodedMessage, 0, encodedMessage.Length); 
            }
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
            SendPIDParams();
        }

        private void SendPIDParams()
        {
            byte[] rawDataLin = UARTProtocol.UartEncode(new SerialCommandSetPID(Pid.PID_LIN, 0, 0, 0, 100, 100, 100));
            byte[] rawDataAng = UARTProtocol.UartEncode(new SerialCommandSetPID(Pid.PID_ANG, 0, 0, 0, 100, 100, 100));

            if (!isLaptop) serialPort1.Write(rawDataLin, 0, rawDataLin.Length);
            if (!isLaptop) serialPort1.Write(rawDataAng, 0, rawDataAng.Length);
        }

        private void SendConsignes()
        {
#pragma warning disable CS8629
            float consVitesseLin = (float)speedLinearUpDown.Value;
            float consVitesseAng = (float)speedAngularUpDown.Value;
#pragma warning restore CS8629

            byte[] rawDataConsLin = UARTProtocol.UartEncode(new SerialCommandSetconsigneLin(consVitesseLin));
            byte[] rawDataConsAng = UARTProtocol.UartEncode(new SerialCommandSetconsigneAng(consVitesseAng));

            if (!isLaptop) serialPort1.Write(rawDataConsLin, 0, rawDataConsLin.Length);
            if (!isLaptop) serialPort1.Write(rawDataConsAng, 0, rawDataConsAng.Length);
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
            byte[] rawDataModeManu = UARTProtocol.UartEncode(new SerialCommandSetRobotMode((byte)0));
            if (!isLaptop) serialPort1.Write(rawDataModeManu, 0, rawDataModeManu.Length);
        }

        private void SendModeManu_Unchecked(object sender, RoutedEventArgs e)
        {
            robot.autoModeActivated = true;
            byte[] rawDataStateModeAuto = UARTProtocol.UartEncode(new SerialCommandSetRobotMode((byte)1));
            if (!isLaptop) serialPort1.Write(rawDataStateModeAuto, 0, rawDataStateModeAuto.Length);
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

            if (robot.autoModeActivated == false)
            {

                if (e.KeyChar == '8')
                {
                    byte[] rawDataStateAvance = UARTProtocol.UartEncode(new SerialCommandSetRobotState((byte)StateRobot.STATE_AVANCE));
                    if (!isLaptop) serialPort1.Write(rawDataStateAvance, 0, rawDataStateAvance.Length);
                }
                else if (e.KeyChar == '4')
                {
                    byte[] rawDataStateGauche = UARTProtocol.UartEncode(new SerialCommandSetRobotState((byte)StateRobot.STATE_TOURNE_SUR_PLACE_GAUCHE));
                    if (!isLaptop) serialPort1.Write(rawDataStateGauche, 0, rawDataStateGauche.Length);
                }
                else if (e.KeyChar == '2')
                {
                    byte[] rawDataStateRecule = UARTProtocol.UartEncode(new SerialCommandSetRobotState((byte)StateRobot.STATE_RECULE));
                    if (!isLaptop) serialPort1.Write(rawDataStateRecule, 0, rawDataStateRecule.Length);
                }
                else if (e.KeyChar == '6')
                {
                    byte[] rawDataStateDroite = UARTProtocol.UartEncode(new SerialCommandSetRobotState((byte)StateRobot.STATE_TOURNE_SUR_PLACE_DROITE));
                    if (!isLaptop) serialPort1.Write(rawDataStateDroite, 0, rawDataStateDroite.Length);
                }
                else if (e.KeyChar == '5')
                {
                    byte[] rawDataStateAttente = UARTProtocol.UartEncode(new SerialCommandSetRobotState((byte)StateRobot.STATE_STOP));
                    if (!isLaptop) serialPort1.Write(rawDataStateAttente, 0, rawDataStateAttente.Length);
                }
            }
        }

        private void IntegerUpDown_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Rejeter l'entrée de consigne si elle n'est pas numérique
            if (!IsTextAllowed(e.Text))
            {
                e.Handled = true;
            }
        }

        private static bool IsTextAllowed(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void IntegerUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
            }
        }
    }
}