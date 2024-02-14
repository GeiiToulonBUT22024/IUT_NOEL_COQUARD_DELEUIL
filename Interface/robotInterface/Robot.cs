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


        public Queue<string> stringListReceived = new Queue<string>();

        public Pid pidLin = new Pid();
        public Pid pidAng = new Pid();


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
}
