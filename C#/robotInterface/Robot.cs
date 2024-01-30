using System;
using System.Collections.Generic;
using System.Linq;
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

        // public Queue<byte> byteListReceived = new Queue<byte>();
        public Queue<string> stringListReceived = new Queue<string>();


        public Robot()
        {

        }
    }
}
