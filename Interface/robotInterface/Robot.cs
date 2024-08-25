using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace robotInterface
{
    public class Robot
    {
        public string receivedText = "";

        public float distanceTelemetreLePen;
        public float distanceTelemetreDroit;
        public float distanceTelemetreCentre;
        public float distanceTelemetreGauche;
        public float distanceTelemetreMelenchon;

        public float consigneGauche;
        public float consigneDroite;

        public byte ledBlanche;
        public byte ledBleue;
        public byte ledOrange;

        public float timestamp;
        public float positionXOdo;
        public float positionYOdo;
        public float angle;
        public float vitLin;
        public float vitAng;

        public float consigneLin;
        public float consigneAng;

        public bool autoModeActivated;
        public int state;
        public int mode;

        public const byte MODE_AUTO = 1;
        public const byte MODE_MANUEL = 0;

        public Queue<string> stringListReceived = new Queue<string>();

        public Pid pidLin = new Pid();
        public Pid pidAng = new Pid();

        public Ghost ghost = new Ghost();

        public Robot()
        {

        }
    }

    public class Pid
    {
        public const byte PID_LIN = 0;
        public const byte PID_ANG = 1;

        public float Kp;
        public float Ki;
        public float Kd;
        public float consigne;
        public float erreur;
        public float corrP;
        public float corrI;
        public float corrD;
        public float erreurPmax;
        public float erreurImax;
        public float erreurDmax;

        public float cmdLin;
        public float cmdAng;

        public Pid()
        {

        }
    }

    public class Ghost
    {
        public float x;
        public float y;
        public float distanceToTarget;
        public float angleToTarget;
        public float theta;
        public float angularSpeed;
        public float ghostPosX;
        public float ghostPosY;

        public Ghost()
        {

        }
    }
}