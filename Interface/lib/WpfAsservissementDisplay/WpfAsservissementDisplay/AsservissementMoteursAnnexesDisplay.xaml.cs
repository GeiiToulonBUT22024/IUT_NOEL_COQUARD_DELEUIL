using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Constants;
using Utilities;

namespace WpfAsservissementDisplay
{
    public partial class AsservissementMoteursAnnexesDisplay : UserControl
    {
        int queueSize = 1;
        FixedSizedQueue<double> commandM5List;
        FixedSizedQueue<double> commandM6List;
        FixedSizedQueue<double> commandM7List;
        FixedSizedQueue<double> commandM8List;

        FixedSizedQueue<double> consigneM5List;
        FixedSizedQueue<double> consigneM6List;
        FixedSizedQueue<double> consigneM7List;
        FixedSizedQueue<double> consigneM8List;

        FixedSizedQueue<double> measuredM5List;
        FixedSizedQueue<double> measuredM6List;
        FixedSizedQueue<double> measuredM7List;
        FixedSizedQueue<double> measuredM8List;

        FixedSizedQueue<double> errorM8List;
        FixedSizedQueue<double> errorM7List;
        FixedSizedQueue<double> errorM6List;
        FixedSizedQueue<double> errorM5List;

        FixedSizedQueue<double> corrPM5List;
        FixedSizedQueue<double> corrPM6List;
        FixedSizedQueue<double> corrPM7List;
        FixedSizedQueue<double> corrPM8List;

        FixedSizedQueue<double> corrIM5List;
        FixedSizedQueue<double> corrIM6List;
        FixedSizedQueue<double> corrIM7List;
        FixedSizedQueue<double> corrIM8List;

        FixedSizedQueue<double> corrDM5List;
        FixedSizedQueue<double> corrDM6List;
        FixedSizedQueue<double> corrDM7List;
        FixedSizedQueue<double> corrDM8List;

        double corrLimitPM5, corrLimitPM6, corrLimitPM7, corrLimitPM8;
        double corrLimitIM5, corrLimitIM6, corrLimitIM7, corrLimitIM8;
        double corrLimitDM5, corrLimitDM6, corrLimitDM7, corrLimitDM8;

        double KpM5, KpM6, KpM7, KpM8;
        double KiM5, KiM6, KiM7, KiM8;
        double KdM5, KdM6, KdM7, KdM8;

        System.Timers.Timer displayTimer;

        AsservissementMode asservModeM5 = AsservissementMode.DisabledM5;
        AsservissementMode asservModeM6 = AsservissementMode.DisabledM6;
        AsservissementMode asservModeM7 = AsservissementMode.DisabledM7;
        AsservissementMode asservModeM8 = AsservissementMode.DisabledM8;

        public AsservissementMoteursAnnexesDisplay()
        {
            InitializeComponent();
            commandM5List = new Utilities.FixedSizedQueue<double>(queueSize);
            commandM6List = new Utilities.FixedSizedQueue<double>(queueSize);
            commandM7List = new Utilities.FixedSizedQueue<double>(queueSize);
            commandM8List = new Utilities.FixedSizedQueue<double>(queueSize);

            consigneM5List = new Utilities.FixedSizedQueue<double>(queueSize);
            consigneM6List = new Utilities.FixedSizedQueue<double>(queueSize);
            consigneM7List = new Utilities.FixedSizedQueue<double>(queueSize);
            consigneM8List = new Utilities.FixedSizedQueue<double>(queueSize);

            measuredM5List = new Utilities.FixedSizedQueue<double>(queueSize);
            measuredM6List = new Utilities.FixedSizedQueue<double>(queueSize);
            measuredM7List = new Utilities.FixedSizedQueue<double>(queueSize);
            measuredM8List = new Utilities.FixedSizedQueue<double>(queueSize);

            errorM5List = new Utilities.FixedSizedQueue<double>(queueSize);
            errorM6List = new Utilities.FixedSizedQueue<double>(queueSize);
            errorM7List = new Utilities.FixedSizedQueue<double>(queueSize);
            errorM8List = new Utilities.FixedSizedQueue<double>(queueSize);

            corrPM5List = new Utilities.FixedSizedQueue<double>(queueSize);
            corrPM6List = new Utilities.FixedSizedQueue<double>(queueSize);
            corrPM7List = new Utilities.FixedSizedQueue<double>(queueSize);
            corrPM8List = new Utilities.FixedSizedQueue<double>(queueSize);

            corrIM5List = new Utilities.FixedSizedQueue<double>(queueSize);
            corrIM6List = new Utilities.FixedSizedQueue<double>(queueSize);
            corrIM7List = new Utilities.FixedSizedQueue<double>(queueSize);
            corrIM8List = new Utilities.FixedSizedQueue<double>(queueSize);

            corrDM5List = new Utilities.FixedSizedQueue<double>(queueSize);
            corrDM6List = new Utilities.FixedSizedQueue<double>(queueSize);
            corrDM7List = new Utilities.FixedSizedQueue<double>(queueSize);
            corrDM8List = new Utilities.FixedSizedQueue<double>(queueSize);

            consigneM5List.Enqueue(0);
            consigneM6List.Enqueue(0);
            consigneM7List.Enqueue(0);
            consigneM8List.Enqueue(0);

            commandM5List.Enqueue(0);
            commandM6List.Enqueue(0);
            commandM7List.Enqueue(0);
            commandM8List.Enqueue(0);

            measuredM5List.Enqueue(0);
            measuredM6List.Enqueue(0);
            measuredM7List.Enqueue(0);
            measuredM8List.Enqueue(0);

            errorM5List.Enqueue(0);
            errorM6List.Enqueue(0);
            errorM7List.Enqueue(0);
            errorM8List.Enqueue(0);

            displayTimer = new Timer(100);
            displayTimer.Elapsed += DisplayTimer_Elapsed;
            displayTimer.Start();
        }

        public void SetMotor5AsservissementMode(AsservissementMode mode)
        {
            asservModeM5 = mode;
            switch (asservModeM5)
            {
                case AsservissementMode.DisabledM5:
                    LabelConsigneM5.Visibility = Visibility.Hidden;
                    LabelErreurM5.Visibility = Visibility.Hidden;
                    LabelCommandM5.Visibility = Visibility.Hidden;
                    LabelCorrPM5.Visibility = Visibility.Hidden;
                    LabelCorrIM5.Visibility = Visibility.Hidden;
                    LabelCorrDM5.Visibility = Visibility.Hidden;
                    break;
                case AsservissementMode.EnabledM5:
                    LabelConsigneM5.Visibility = Visibility.Visible;
                    LabelErreurM5.Visibility = Visibility.Visible;
                    LabelCommandM5.Visibility = Visibility.Visible;
                    LabelCorrPM5.Visibility = Visibility.Visible;
                    LabelCorrIM5.Visibility = Visibility.Visible;
                    LabelCorrDM5.Visibility = Visibility.Visible;
                    break;
            }
        }
    
        public void SetMotor6AsservissementMode(AsservissementMode mode)
        {
            asservModeM6 = mode;

            switch (asservModeM6)
            {
                case AsservissementMode.DisabledM6:
                    LabelConsigneM6.Visibility = Visibility.Hidden;
                    LabelErreurM6.Visibility = Visibility.Hidden;
                    LabelCommandM6.Visibility = Visibility.Hidden;
                    LabelCorrPM6.Visibility = Visibility.Hidden;
                    LabelCorrIM6.Visibility = Visibility.Hidden;
                    LabelCorrDM6.Visibility = Visibility.Hidden;
                    break;
                case AsservissementMode.EnabledM6:
                    LabelConsigneM6.Visibility = Visibility.Visible;
                    LabelErreurM6.Visibility = Visibility.Visible;
                    LabelCommandM6.Visibility = Visibility.Visible;
                    LabelCorrPM6.Visibility = Visibility.Visible;
                    LabelCorrIM6.Visibility = Visibility.Visible;
                    LabelCorrDM6.Visibility = Visibility.Visible;
                    break;
            }
        }

        public void SetMotor7AsservissementMode(AsservissementMode mode)
        {
            asservModeM7 = mode;

            switch (asservModeM7)
            {
                case AsservissementMode.DisabledM7:
                    LabelConsigneM7.Visibility = Visibility.Hidden;
                    LabelErreurM7.Visibility = Visibility.Hidden;
                    LabelCommandM7.Visibility = Visibility.Hidden;
                    LabelCorrPM7.Visibility = Visibility.Hidden;
                    LabelCorrIM7.Visibility = Visibility.Hidden;
                    LabelCorrDM7.Visibility = Visibility.Hidden;
                    break;
                case AsservissementMode.EnabledM7:
                    LabelConsigneM7.Visibility = Visibility.Visible;
                    LabelErreurM7.Visibility = Visibility.Visible;
                    LabelCommandM7.Visibility = Visibility.Visible;
                    LabelCorrPM7.Visibility = Visibility.Visible;
                    LabelCorrIM7.Visibility = Visibility.Visible;
                    LabelCorrDM7.Visibility = Visibility.Visible;
                    break;
            }
        }
        public void SetMotor8AsservissementMode(AsservissementMode mode)
        {
            asservModeM8 = mode;

            switch (asservModeM8)
            {
                case AsservissementMode.DisabledM8:
                    LabelConsigneM8.Visibility = Visibility.Hidden;
                    LabelErreurM8.Visibility = Visibility.Hidden;
                    LabelCommandM8.Visibility = Visibility.Hidden;
                    LabelCorrPM8.Visibility = Visibility.Hidden;
                    LabelCorrIM8.Visibility = Visibility.Hidden;
                    LabelCorrDM8.Visibility = Visibility.Hidden;
                    break;
                case AsservissementMode.EnabledM8:
                    LabelConsigneM8.Visibility = Visibility.Visible;
                    LabelErreurM8.Visibility = Visibility.Visible;
                    LabelCommandM8.Visibility = Visibility.Visible;
                    LabelCorrPM8.Visibility = Visibility.Visible;
                    LabelCorrIM8.Visibility = Visibility.Visible;
                    LabelCorrDM8.Visibility = Visibility.Visible;
                    break;
            }
        }

        public void SetTitle(string titre)
        {
            LabelTitre.Content = titre;
        }

        private void DisplayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                UpdateDisplay();
            }));
        }

        public void UpdateDisplay()
        {
            LabelConsigneM5.Content = consigneM5List.Average().ToString("N2");
            LabelConsigneM6.Content = consigneM6List.Average().ToString("N2");
            LabelConsigneM7.Content = consigneM7List.Average().ToString("N2");
            LabelConsigneM8.Content = consigneM8List.Average().ToString("N2");

            LabelMeasureM5.Content = measuredM5List.Average().ToString("N2");
            LabelMeasureM6.Content = measuredM6List.Average().ToString("N2");
            LabelMeasureM7.Content = measuredM7List.Average().ToString("N2");
            LabelMeasureM8.Content = measuredM8List.Average().ToString("N2");

            LabelErreurM5.Content = errorM5List.Average().ToString("N2");
            LabelErreurM6.Content = errorM6List.Average().ToString("N2");
            LabelErreurM7.Content = errorM7List.Average().ToString("N2");
            LabelErreurM8.Content = errorM8List.Average().ToString("N2");

            LabelCommandM5.Content = commandM5List.Average().ToString("N2");
            LabelCommandM6.Content = commandM6List.Average().ToString("N2");
            LabelCommandM7.Content = commandM7List.Average().ToString("N2");
            LabelCommandM8.Content = commandM8List.Average().ToString("N2");

            LabelKpM5.Content = KpM5.ToString("N2");
            LabelKpM6.Content = KpM6.ToString("N2");
            LabelKpM7.Content = KpM7.ToString("N2");
            LabelKpM8.Content = KpM8.ToString("N2");

            LabelKiM5.Content = KiM5.ToString("N2");
            LabelKiM6.Content = KiM6.ToString("N2");
            LabelKiM7.Content = KiM7.ToString("N2");
            LabelKiM8.Content = KiM8.ToString("N2");

            LabelKdM5.Content = KdM5.ToString("N2");
            LabelKdM6.Content = KdM6.ToString("N2");
            LabelKdM7.Content = KdM7.ToString("N2");
            LabelKdM8.Content = KdM8.ToString("N2");

            LabelCorrMaxPM5.Content = corrLimitPM5.ToString("N2");
            LabelCorrMaxPM6.Content = corrLimitPM6.ToString("N2");
            LabelCorrMaxPM7.Content = corrLimitPM7.ToString("N2");
            LabelCorrMaxPM8.Content = corrLimitPM8.ToString("N2");

            LabelCorrMaxIM5.Content = corrLimitIM5.ToString("N2");
            LabelCorrMaxIM6.Content = corrLimitIM6.ToString("N2");
            LabelCorrMaxIM7.Content = corrLimitIM7.ToString("N2");
            LabelCorrMaxIM8.Content = corrLimitIM8.ToString("N2");

            LabelCorrMaxDM5.Content = corrLimitDM5.ToString("N2");
            LabelCorrMaxDM6.Content = corrLimitDM6.ToString("N2");
            LabelCorrMaxDM7.Content = corrLimitDM7.ToString("N2");
            LabelCorrMaxDM8.Content = corrLimitDM8.ToString("N2");


            if (corrPM5List.Count > 0)
            {
                LabelCorrPM5.Content = corrPM5List.Average().ToString("N2");
                LabelCorrIM5.Content = corrIM5List.Average().ToString("N2");
                LabelCorrDM5.Content = corrDM5List.Average().ToString("N2");
            }

            if (corrPM6List.Count > 0)
            {
                LabelCorrPM6.Content = corrPM6List.Average().ToString("N2");
                LabelCorrIM6.Content = corrIM6List.Average().ToString("N2");
                LabelCorrDM6.Content = corrDM6List.Average().ToString("N2");
            }

            if (corrPM7List.Count > 0)
            {
                LabelCorrPM7.Content = corrPM7List.Average().ToString("N2");
                LabelCorrIM7.Content = corrIM7List.Average().ToString("N2");
                LabelCorrDM7.Content = corrDM7List.Average().ToString("N2");
            }

            if (corrPM8List.Count > 0)
            {
                LabelCorrPM8.Content = corrPM8List.Average().ToString("N2");
                LabelCorrIM8.Content = corrIM8List.Average().ToString("N2");
                LabelCorrDM8.Content = corrDM8List.Average().ToString("N2");
            }
        }


        public void UpdateMotor5SpeedConsigneValues(double consigneM5)
        {
            consigneM5List.Enqueue(consigneM5);
        }
        public void UpdateMotor6SpeedConsigneValues(double consigneM6)
        {
            consigneM6List.Enqueue(consigneM6);
        }
        public void UpdateMotor7SpeedConsigneValues(double consigneM7)
        {
            consigneM7List.Enqueue(consigneM7);
        }
        public void UpdateMotor8SpeedConsigneValues(double consigneM8)
        {
            consigneM8List.Enqueue(consigneM8);
        }


        public void UpdateIndependantSpeedCommandValues(double commandM5, double commandM6, double commandM7, double commandM8)
        {
            commandM5List.Enqueue(commandM5);
            commandM6List.Enqueue(commandM6);
            commandM7List.Enqueue(commandM7);
            commandM8List.Enqueue(commandM8);
        }

        public void UpdateIndependantOdometrySpeed(double valueM5, double valueM6, double valueM7, double valueM8)
        {
            measuredM5List.Enqueue(valueM5);
            measuredM6List.Enqueue(valueM6);
            measuredM7List.Enqueue(valueM7);
            measuredM8List.Enqueue(valueM8);
        }

        public void UpdateMotor5SpeedErrorValues(double errorM5)
        {
            errorM5List.Enqueue(errorM5);
        }
        public void UpdateMotor6SpeedErrorValues(double errorM6)
        {
            errorM6List.Enqueue(errorM6);
        }
        public void UpdateMotor7SpeedErrorValues(double errorM7)
        {
            errorM7List.Enqueue(errorM7);
        }
        public void UpdateMotor8SpeedErrorValues(double errorM8)
        {
            errorM8List.Enqueue(errorM8);
        }

        public void UpdateMotor5SpeedCommandValues(double command)
        {
            commandM5List.Enqueue(command);
        }
        public void UpdateMotor6SpeedCommandValues(double command)
        {
            commandM6List.Enqueue(command);
        }
        public void UpdateMotor7SpeedCommandValues(double command)
        {
            commandM7List.Enqueue(command);
        }
        public void UpdateMotor8SpeedCommandValues(double command)
        {
            commandM8List.Enqueue(command);
        }

        public void UpdateMotor5SpeedCorrectionValues(double corrPM5, double corrIM5, double corrDM5)
        {
            corrPM5List.Enqueue(corrPM5);
            corrIM5List.Enqueue(corrIM5);
            corrDM5List.Enqueue(corrDM5);
        }
        public void UpdateMotor6SpeedCorrectionValues(double corrPM6, double corrIM6, double corrDM6)
        {
            corrPM6List.Enqueue(corrPM6);
            corrIM6List.Enqueue(corrIM6);
            corrDM6List.Enqueue(corrDM6);
        }
        public void UpdateMotor7SpeedCorrectionValues(double corrPM7, double corrIM7, double corrDM7)
        {
            corrPM7List.Enqueue(corrPM7);
            corrIM7List.Enqueue(corrIM7);
            corrDM7List.Enqueue(corrDM7);
        }
        public void UpdateMotor8SpeedCorrectionValues(double corrPM8, double corrIM8, double corrDM8)
        {
            corrPM8List.Enqueue(corrPM8);
            corrIM8List.Enqueue(corrIM8);
            corrDM8List.Enqueue(corrDM8);
        }

        public void UpdateIndependantSpeedCorrectionGains(double KpM5, double KpM6, double KpM7, double KpM8,
            double KiM5, double KiM6, double KiM7, double KiM8,
            double KdM5, double KdM6, double KdM7, double KdM8)
        {
            this.KpM5 = KpM5;
            this.KpM6 = KpM6;
            this.KpM7 = KpM7;
            this.KpM8 = KpM8;
            this.KiM5 = KiM5;
            this.KiM6 = KiM6;
            this.KiM7 = KiM7;
            this.KiM8 = KiM8;
            this.KdM5 = KdM5;
            this.KdM6 = KdM6;
            this.KdM7 = KdM7;
            this.KdM8 = KdM8;
        }

        public void UpdateMotor5SpeedCorrectionGains(double Kp, double Ki, double Kd)
        {
            this.KpM5 = Kp;
            this.KiM5 = Ki;
            this.KdM5 = Kd;
        }

        public void UpdateMotor6SpeedCorrectionGains(double Kp, double Ki, double Kd)
        {
            this.KpM6 = Kp;
            this.KiM6 = Ki;
            this.KdM6 = Kd;
        }

        public void UpdateMotor7SpeedCorrectionGains(double Kp, double Ki, double Kd)
        {
            this.KpM7 = Kp;
            this.KiM7 = Ki;
            this.KdM7 = Kd;
        }

        public void UpdateMotor8SpeedCorrectionGains(double Kp, double Ki, double Kd)
        {
            this.KpM8 = Kp;
            this.KiM8 = Ki;
            this.KdM8 = Kd;
        }


        public void UpdateIndependantSpeedCorrectionLimits(double corrLimitPM5, double corrLimitPM6, double corrLimitPM7, double corrLimitPM8,
            double corrLimitIM5, double corrLimitIM6, double corrLimitIM7, double corrLimitIM8,
            double corrLimitDM5, double corrLimitDM6, double corrLimitDM7, double corrLimitDM8)
        {
            this.corrLimitPM5 = corrLimitPM5;
            this.corrLimitPM6 = corrLimitPM6;
            this.corrLimitPM7 = corrLimitPM7;
            this.corrLimitPM8 = corrLimitPM8;
            this.corrLimitIM5 = corrLimitIM5;
            this.corrLimitIM6 = corrLimitIM6;
            this.corrLimitIM7 = corrLimitIM7;
            this.corrLimitIM8 = corrLimitIM8;
            this.corrLimitDM5 = corrLimitDM5;
            this.corrLimitDM6 = corrLimitDM6;
            this.corrLimitDM7 = corrLimitDM7;
            this.corrLimitDM8 = corrLimitDM8;
        }

        public void UpdateMotor5CorrectionLimits(double corrLimitP, double corrLimitI, double corrLimitD)
        {
            this.corrLimitPM5 = corrLimitP;
            this.corrLimitIM5 = corrLimitI;
            this.corrLimitDM5 = corrLimitD;
        }

        public void UpdateMotor6CorrectionLimits(double corrLimitP, double corrLimitI, double corrLimitD)
        {
            this.corrLimitPM6 = corrLimitP;
            this.corrLimitIM6 = corrLimitI;
            this.corrLimitDM6 = corrLimitD;
        }

        public void UpdateMotor7CorrectionLimits(double corrLimitP, double corrLimitI, double corrLimitD)
        {
            this.corrLimitPM7 = corrLimitP;
            this.corrLimitIM7 = corrLimitI;
            this.corrLimitDM7 = corrLimitD;
        }

        public void UpdateMotor8CorrectionLimits(double corrLimitP, double corrLimitI, double corrLimitD)
        {
            this.corrLimitPM8 = corrLimitP;
            this.corrLimitIM8 = corrLimitI;
            this.corrLimitDM8 = corrLimitD;
        }
    }
}