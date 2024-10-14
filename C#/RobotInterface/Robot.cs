using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotInterface
{
    public class Robot
    {
        public string receivedText = "";
        public Queue<byte> byteListReceived;
        public float distanceTelemetreDroit;
        public float distanceTelemetreCentre;
        public float distanceTelemetreGauche;


        public long timestamp;
        public float positionXOdo;
        public float positionYOdo;
        public float angleRadOdo;

        public float vitesseLineaire;
        public float vitesseAngulaire;

        public float positionXGhosto;
        public float positionYGhosto;

        public float vitesseAngGhosto;

        public float distanceToTargetGhosto;
        public float angleToTargetGhosto;
        public float thetaGhosto;
        public float vitesseLineaireGhosto;



        public List<string> TextDebugAutoMode;
        public List<string> TextToPrint;
        public Robot()
        {
            byteListReceived = new Queue<byte>();
            TextToPrint = new List<string>();
            TextDebugAutoMode = new List<string>();
        }
    }
}
