namespace Constants
{
    public enum Commands
    {
        #region Commandes générales
#pragma warning disable CS1591

        /// <summary>
        /// Le principe d'attribution des commandes est le suivant :
        /// On distingue :
        /// Les commandes R2PC : Robot to Computer - elles permettent d'envoyer des infos ou des ordres au PC
        /// Elles sont dans le range 0x0100 - 0x1FF
        /// Les acknowledgment ont un format particulier : on rajoute 0x1000 à la commande initiale 
        /// Les commandes PC2R : Computer to Robot - elles permettent d'envoyer des infos ou des ordres au robot
        /// Elles sont dans le range 0x0200 - 0x2FF
        /// </summary>

        R2PC_WelcomeMessage = 0x0100,                                   //Pas de payload
        R2PC_ErrorMessage = 0x0101,                                     //Payload de taille variable

        R2PC_IMUData = 0x0110,                                          //Timestamp(4L) - AccX(4F) - AccY(4F) - AccZ(4F) - GyroX(4F) - GyroY(4F) - GyroZ(4F)
        R2PC_IOMonitoring = 0x0120,                                     //Timestamp(4L) - IO1-IO8 (1)
        R2PC_PowerMonitoring = 0x0130,                                  //Timestamp(4L) - BattCmdVoltage(4F) - BattCmdCurrent(4F) - BattPwrVoltage(4F) - BattPwrCurrent(4F)
        R2PC_EncoderRawData = 0x0140,                                   //Timestamp(4L) - Enc Motor 1 Value(4L) - ... - Enc Motor 7 Value(4L))

        R2PC_SpeedPolarAndIndependantOdometry = 0x0150,                 //Timestamp(4L) - Vx(4F) - Vy(4F) - VTheta(4F) - VM1(4F) - VM2(4F) - VM3(4F) - VM4(4F)
        R2PC_SpeedAuxiliaryOdometry = 0x0151,                           //Timestamp(4L) - VM5(4F) - VM6(4F) - VM7(4F) - VM8(4F)
        R2PC_4WheelsSpeedPolarPidCommandErrorCorrectionConsigne = 0x0152,          //Timestamp(4L) - ErrX(4F) - ErrY(4F) - ErrTh(4F) - CorrX(4F) - CorrY(4F) - CorrTh(4F) - ConsX(4F) - ConsY(4F) - ConsTh(4F)
        R2PC_4WheelsSpeedIndependantPidCommandErrorCorrectionConsigne = 0x0153,    //Timestamp(4L) - ErrM1(4F) - ErrM2(4F) - ErrM3(4F) - ErrM4(4F) - CorrM1(4F) - CorrM2(4F) - CorrM3(4F) - CorrM4(4F) - ConsM(4F) - ConsM2(4F) - ConsM3(4F) - ConsM4(4F)
        R2PC_4WheelsSpeedPolarPidCorrections = 0x0154,                       //Timestamp(4L) - CorrPx(4F) - CorrIx(4F) - CorrDx(4F) - CorrPy(4F) - CorrIy(4F) - CorrDy(4F) - CorrPTh(4F) - CorrITh(4F) - CorrDTh(4F)  
        R2PC_4WheelsSpeedIndependantPidCorrections = 0x0155,                 //Timestamp(4L) - CorrPM1(4F) - CorrIM1(4F) - CorrDM1(4F) - CorrPM2(4F) - CorrIM2(4F) - CorrDM2(4F) - CorrPM3(4F) - CorrIM3(4F) - CorrDM3(4F) - CorrPM4(4F) - CorrIM4(4F) - CorrDM4(4F)
        R2PC_SpeedAuxiliaryMotorsConsignes = 0x0156,                    //Timestamp(4L) - Consigne Motor 5(4F) - Consigne Motor 6(4F) - Consigne Motor 7(4F) - Consigne Motor 8(4F) )
        R2PC_SpeedMotor5PidCorrections = 0x0157,                            //Timestamp(4L) - CorrPM5(4F) - CorrIM5(4F) - CorrDM5(4F)
        R2PC_SpeedMotor6PidCorrections = 0x0158,                            //Timestamp(4L) - CorrPM6(4F) - CorrIM6(4F) - CorrDM6(4F)
        R2PC_SpeedMotor7PidCorrections = 0x0159,                            //Timestamp(4L) - CorrPM7(4F) - CorrIM7(4F) - CorrDM7(4F)
        R2PC_SpeedMotor8PidCorrections = 0x015A,                            //Timestamp(4L) - CorrPM8(4F) - CorrIM8(4F) - CorrDM8(4F)

        R2PC_MotorCurrentsMonitoring = 0x0160,                          //Timestamp(4L) - Motor Current 1 (4F) - ... - Motor Current 7 (4F)
        R2PC_2WheelsSpeedPolarPidCommandErrorCorrectionConsigne = 0x0162,          //Timestamp(4L) - ErrX(4F) - ErrTh(4F) - CorrX(4F) - CorrTh(4F) - ConsX(4F) - ConsTh(4F)
        R2PC_2WheelsSpeedIndependantPidCommandErrorCorrectionConsigne = 0x0163,    //Timestamp(4L) - ErrM1(4F) - ErrM2(4F) - CorrM1(4F) - CorrM2(4F) - ConsM1(4F) - ConsM2(4F)
        R2PC_2WheelsSpeedPolarPidCorrections = 0x0164,                       //Timestamp(4L) - CorrPx(4F) - CorrIx(4F) - CorrDx(4F) - CorrPTh(4F) - CorrITh(4F) - CorrDTh(4F)  
        R2PC_2WheelsSpeedIndependantPidCorrections = 0x0165,                 //Timestamp(4L) - CorrPM1(4F) - CorrIM1(4F) - CorrDM1(4F) - CorrPM2(4F) - CorrIM2(4F) - CorrDM2(4F)
        R2PC_SpeedMotor5PidErrorCorrectionsConsigne = 0x0166,                //Timestamp(4L) - ErrM5(4F) - CorrM5(4F) - ConsM5(4F)
        R2PC_SpeedMotor6PidErrorCorrectionsConsigne = 0x0167,                //Timestamp(4L) - ErrM6(4F) - CorrM6(4F) - ConsM6(4F)
        R2PC_SpeedMotor7PidErrorCorrectionsConsigne = 0x0168,                //Timestamp(4L) - ErrM7(4F) - CorrM7(4F) - ConsM7(4F)
        R2PC_SpeedMotor8PidErrorCorrectionsConsigne = 0x0169,                //Timestamp(4L) - ErrM8(4F) - CorrM8(4F) - ConsM8(4F)


        //Retour des commandes d'enable du PC
        R2PC_IndividualMotorsEnableDisableStatus = 0x017E,                 //MotorNum (1 Byte) - EnableDisable (1 Byte)
        R2PC_PrehensionMotorsEnableDisableStatus = 0x017F,                 //Enable-Disable (1 Byte)
        R2PC_IOPollingEnableStatus = 0x0180,                               //Enable-Disable (1 Byte)
        R2PC_PowerMonitoringEnableStatus = 0x0181,                         //Enable-Disable (1 Byte)
        R2PC_EncoderRawMonitoringEnableStatus = 0x0182,                    //Enable-Disable (1 Byte)
        R2PC_AsservissementModeStatus = 0x0183,                            //Enable-Disable (1 Byte)
        R2PC_SpeedPIDEnableDebugErrorCorrectionConsigneStatus = 0x0184,    //Enable-Disable (1 Byte)
        R2PC_SpeedPIDEnableDebugInternalStatus = 0x0185,                       //Enable-Disable (1 Byte)
        R2PC_SpeedConsigneMonitoringEnableStatus = 0x0186,                 //Enable-Disable (1 Byte)
        R2PC_PropulsionMotorsEnableDisableStatus = 0x0187,                 //Enable-Disable (1 Byte)
        R2PC_MotorCurrentMonitoringEnableStatus = 0x0188,                  //Enable-Disable (1 Byte)
        R2PC_TirEnableDisableStatus = 0x0189,                              //Enable-Disable (1 Byte)
        R2PC_UARTRawDataForwardStatus = 0x018A,                            //Enable-Disable (1 Byte)
        R2PC_IOAnalogPollingEnableStatus = 0x018B,                         //Enable-Disable (1 Byte)
        R2PC_AsservissementMotor5ModeStatus= 0x018C,                       //Enable-Disable (1 Byte)
        R2PC_AsservissementMotor6ModeStatus = 0x018D,                       //Enable-Disable (1 Byte)
        R2PC_AsservissementMotor7ModeStatus = 0x018E,                       //Enable-Disable (1 Byte)
        R2PC_AsservissementMotor8ModeStatus = 0x018F,                       //Enable-Disable (1 Byte)

        //Retour des datas RAW recues sur les Uarts
        R2PC_Uart1ForwardRX = 0x0380,                                      //RAW Data (1 to 64 Bytes)
        R2PC_Uart2ForwardRX = 0x0390,                                      //RAW Data (1 to 64 Bytes)
        R2PC_Uart3ForwardRX = 0x03A0,                                      //RAW Data (1 to 64 Bytes)
        R2PC_Uart4ForwardRX = 0x03B0,                                      //RAW Data (1 to 64 Bytes)

        /// <summary>
        /// PC to Robot commands
        /// </summary>
        PC2R_EmergencyStop = 0x0200,

        PC2R_IOPollingEnable = 0x0220,                                  //Enable-Disable (1 Byte)

        PC2R_IOPollingSetFrequency = 0x0221,                            //Frequency (1 Byte)
        PC2R_IOAnalogPollingEnable = 0x0222,                            //Enable-Disable (1 Byte)
        PC2R_ForwardUartRAWDataEnable = 0x0223,                            //Enable-Disable (1 Byte)
        PC2R_PowerMonitoringEnable = 0x0230,                            //Enable-Disable (1 Byte)        

        PC2R_EncoderRawMonitoringEnable = 0x0240,                       //Enable-Disable (1 Byte)
        PC2R_OdometryPointToMeter = 0x0241,                             //PointToMeter (4F)
        PC2R_PropulsionSpeedToPercent = 0x0242,                         //PropulsionSpeedToPercent (4F)
        PC2R_4WheelsToPolarMatrixSet = 0x0243,                          //Mx1 (4F) - Mx2 (4F) - Mx3 (4F) - Mx4 (4F) - My1 (4F) - My2 (4F) - My3 (4F) - My4 (4F) - Mtheta1 (4F) - Mtheta2 (4F) - Mtheta3 (4F) - Mtheta4 (4F)
        PC2R_2WheelsAngleSet = 0x0244,                                  //AngleMotor1 (4F) - AngleMotor2 (4F)
        PC2R_2WheelsToPolarMatrixSet = 0x0245,                          //Mx1 (4F) - Mx2 (4F) - Mtheta1 (4F) - Mtheta2 (4F)
        PC2R_PolarTo4WheelsMatrixSet = 0x0246,                          //M1x (4F) - M1y (4F) - M1theta (4F) - M2x (4F) - M2y (4F) - M2theta (4F) - M3x (4F) - M3y (4F) - M3theta (4F) - M4x (4F) - M4y (4F) - M4theta (4F) 
        PC2R_M5SpeedToPercent = 0x0247,                                 //SpeedToPercent (4F)
        PC2R_M6SpeedToPercent = 0x0248,                                 //SpeedToPercent (4F)
        PC2R_M7SpeedToPercent = 0x0249,                                 //SpeedToPercent (4F)
        PC2R_M8SpeedToPercent = 0x024A,                                 //SpeedToPercent (4F)

        PC2R_SetAsservissementMode = 0x0250,                             //Mode (1 Byte : Disabled=0 - Polarie = 1 - Independant = 2)
        PC2R_SpeedPIDEnableDebugErrorCorrectionConsigne = 0x251,        //Enable-Disable (1 Byte)
        PC2R_SpeedPIDEnableDebugInternal = 0x0252,                      //Enable-Disable (1 Byte)
        PC2R_SpeedConsigneMonitoringEnable = 0x0253,                    //Enable-Disable (1 Byte)
        PC2R_4WheelsPolarSpeedPIDSetGains = 0x0254,                     //KpX(4F) - KiX(4F) - KdX(4F) - idem en Y, en Theta, puis en LimitX, LimitY et Limit Theta : total 72 octets
        PC2R_4WheelsIndependantSpeedPIDSetGains = 0x0255,               //KpM1(4F) - KiM1(4F) - KdM1(4F) - idem en M2, M3 et M4, puis en LimitM1, LimitM2, LimitM3 et Limit M4 : total 96 octets
        PC2R_SpeedPolarSetConsigne = 0x0256,                            //Vx(4F) - Vy(4F) - VTh(4F)
        PC2R_SpeedIndividualMotorSetConsigne = 0x0257,                  //Numero Moteur (1 byte) - VMoteur(4F)
        PC2R_SpeedPIDReset = 0x0258,                                    //Pas de payload
        PC2R_SetAsservissementModeMotor5 = 0x0259,                      //Mode (1 Byte : Disabled=25 Enabled=35)
        PC2R_SetAsservissementModeMotor6 = 0x025A,                      //Mode (1 Byte : Disabled=26 Enabled=36)
        PC2R_SetAsservissementModeMotor7 = 0x025B,                      //Mode (1 Byte : Disabled=27 Enabled=37)
        PC2R_SetAsservissementModeMotor8 = 0x025C,                      //Mode (1 Byte : Disabled=28 Enabled=38)

        PC2R_IndividualMotorsEnableDisable = 0x025E,                    //MotorNum (1 Byte) - Enable-Disable (1 Byte)
        PC2R_PrehensionMotorsEnableDisable = 0x025F,                    //Enable-Disable (1 Byte)
        PC2R_PropulsionMotorsEnableDisable = 0x0260,                    //Enable-Disable (1 Byte)
        PC2R_MotorCurrentMonitoringEnable = 0x0261,                     //Enable-Disable (1 Byte)
        PC2R_2WheelsPolarSpeedPIDSetGains = 0x0264,                     //KpX(4F) - KiX(4F) - KdX(4F) - idem en Theta, puis en LimitX et Limit Theta : total 48 octets
        PC2R_2WheelsIndependantSpeedPIDSetGains = 0x0265,               //KpM1(4F) - KiM1(4F) - KdM1(4F) - idem en M2, puis en LimitM1, LimitM2 : total 48 octets
        PC2R_Motor5PIDSetGains = 0x0266,                                //KpM5(4F) - KiM5(4F) - KdM5(4F) - idem en LimitPM5(4F), LimitIM5(4F), LimitDM5(4F) : total 24 octets
        PC2R_Motor6PIDSetGains = 0x0267,                                //KpM6(4F) - KiM6(4F) - KdM6(4F) - idem en LimitPM6(4F), LimitIM6(4F), LimitDM6(4F) : total 24 octets
        PC2R_Motor7PIDSetGains = 0x0268,                                //KpM7(4F) - KiM7(4F) - KdM7(4F) - idem en LimitPM7(4F), LimitIM7(4F), LimitDM7(4F) : total 24 octets
        PC2R_Motor8PIDSetGains = 0x0269,                                //KpM8(4F) - KiM8(4F) - KdM8(4F) - idem en LimitPM8(4F), LimitIM8(4F), LimitDM8(4F) : total 24 octets

        PC2R_TirEnableDisable = 0x0270,                                 //Enable-Disable (1 Byte)
        PC2R_TirCommand = 0x0271,                                       //Duree Pulse Coil 1 (2) - .. - Duree Pulse Coil 4 (2) - Offset Pulse Coil 2 (2) - .. - Offset Pulse Coil 4 (2) : total 14 bytes
        PC2R_TirMoveUp = 0x0272,                                        //Pas de payload
        PC2R_TirMoveDown = 0x0273,                                      //Pas de payload

        PC2R_UART1_FORWARD = 0x0280,                                    //Payload variable (uart1)
        PC2R_UART1_SET_BAUDRATE = 0x0281,                               //Baudrate(4L)
        PC2R_UART2_FORWARD = 0x0290,                                    //Payload variable (uart2)
        PC2R_UART2_SET_BAUDRATE = 0x0291,                               //Baudrate(4L)
        PC2R_UART3_FORWARD = 0x02A0,                                    //Payload variable (uart3)
        PC2R_UART3_SET_BAUDRATE = 0x02A1,                               //Baudrate(4L)
        PC2R_UART4_FORWARD = 0x02B0,                                    //Payload variable (uart4)
        PC2R_UART4_SET_BAUDRATE = 0x02B1,                               //Baudrate(4L)


#pragma warning restore CS1591
        #endregion
        #region Commandes de la RoboCup

        /// <summary>Arrêt de jeu</summary>
        STOP = 'S',
        /// <summary>Prise ou reprise de jeu</summary>
        START = 's',
        /// <summary>Envoyé pour signaler une connection établie</summary>
        WELCOME = 'W',
        /// <summary>Commande inconnue.</summary> TODO
        WORLD_STATE = 'w',
        /// <summary>Remise à zéro du match</summary>
        RESET = 'Z',
        /// <summary>Reserved for RefBox debugging</summary>
        TESTMODE_ON = 'U',
        /// <summary>Reserved for RefBox debugging</summary>
        TESTMODE_OFF = 'u',
        /// <summary>Carton jaune Magenta</summary>
        YELLOW_CARD_MAGENTA = 'y',
        /// <summary>Carton jaune Cyan</summary>
        YELLOW_CARD_CYAN = 'Y',
        /// <summary>Carton rouge Magenta</summary>
        RED_CARD_MAGENTA = 'r',
        /// <summary>Carton rouge Cyan</summary>
        RED_CARD_CYAN = 'R',
        /// <summary>Commande inconnue.</summary> TODO
        DOUBLE_YELLOW_IN_MAGENTA = 'j',
        /// <summary>Commande inconnue.</summary> TODO
        DOUBLE_YELLOW_IN_CYAN = 'J',
        /// <summary>Début de la première mi-temps</summary>
        FIRST_HALF = '1',
        /// <summary>Début de la seconde mi-temps</summary>
        SECOND_HALF = '2',
        /// <summary>Début de la première mi-temps du temps additionnel</summary>
        FIRST_HALF_OVERTIME = '3',
        /// <summary>Début de la seconde mi-temps du temps additionnel</summary>
        SECOND_HALF_OVERTIME = '4',
        /// <summary>Fin de la première mi-temps (normal ou additionnel)</summary>
        HALF_TIME = 'h',
        /// <summary>Fin de la seconde mi-temps (normal ou additionnel)</summary>
        END_GAME = 'e',
        /// <summary>Commande inconnue.</summary> TODO
        GAMEOVER = 'z',
        /// <summary>Commande inconnue.</summary> TODO
        PARKING = 'L',
        /// <summary>But+ Magenta</summary>
        GOAL_MAGENTA = 'a',
        /// <summary>But+ Cyan</summary>
        GOAL_CYAN = 'A',
        /// <summary>But- Magenta</summary>
        SUBGOAL_MAGENTA = 'd',
        /// <summary>But- Cyan</summary>
        SUBGOAL_CYAN = 'D',
        /// <summary>Coup d'envoi Magenta</summary>
        KICKOFF_MAGENTA = 'k',
        /// <summary>Coup d'envoi Cyan</summary>
        KICKOFF_CYAN = 'K',
        /// <summary>Coup franc Magenta</summary>
        FREEKICK_MAGENTA = 'f',
        /// <summary>Coup franc Cyan</summary>
        FREEKICK_CYAN = 'F',
        /// <summary>Coup franc depuis le goal Magenta</summary>
        GOALKICK_MAGENTA = 'g',
        /// <summary>Coup franc depuis le goal Cyan</summary>
        GOALKICK_CYAN = 'G',
        /// <summary>Touche Magenta</summary>
        THROWIN_MAGENTA = 't',
        /// <summary>Touche Cyan</summary>
        THROWIN_CYAN = 'T',
        /// <summary>Corner Magenta</summary>
        CORNER_MAGENTA = 'c',
        /// <summary>Corner Cyan</summary>
        CORNER_CYAN = 'C',
        /// <summary>Penalty Magenta</summary>
        PENALTY_MAGENTA = 'p',
        /// <summary>Penalty Cyan</summary>
        PENALTY_CYAN = 'P',
        /// <summary>Balle lachée</summary>
        DROPPED_BALL = 'N',
        /// <summary>Robot parti en réparation Magenta</summary>
        REPAIR_OUT_MAGENTA = 'o',
        /// <summary>Robot parti en réparation Cyan</summary>
        REPAIR_OUT_CYAN = 'O',
        /// <summary>Commande inconnue.</summary>
        REPAIR_IN_MAGENTA = 'i',
        /// <summary>Commande inconnue.</summary>
        REPAIR_IN_CYAN = 'I'

        #endregion
    }

    public enum UARTConcentratorCommands:ushort
    {
#pragma warning disable CS1591
        CONC2PC_Uart1ForwardRX = 0x0101,
        CONC2PC_Uart2ForwardRX = 0x0102,
        CONC2PC_Uart3ForwardRX = 0x0103,
        CONC2PC_Uart4ForwardRX = 0x0104,
        CONC2PC_Uart5ForwardRX = 0x0105,
        CONC2PC_Uart6ForwardRX = 0x0106,
        CONC2PC_ErrorMessage = 0xEEEE,
#pragma warning restore CS1591
    }
    public enum AsservissementMode
    {
        Off4Wheels = 0,
        Polar4Wheels = 1,
        Independant4Wheels = 2,
        Off2Wheels = 10,
        Polar2Wheels = 11,
        Independant2Wheels = 12,
        OffM5 = 13,
        DisabledM5 = 25,
        DisabledM6 = 26,
        DisabledM7 = 27,
        DisabledM8 = 28,
        EnabledM5 = 35,
        EnabledM6 = 36,
        EnabledM7 = 37,
        EnabledM8 = 38,
    }
    public enum ActiveMode
    {
        Disabled = 0,
        Enabled = 1
    }
}
