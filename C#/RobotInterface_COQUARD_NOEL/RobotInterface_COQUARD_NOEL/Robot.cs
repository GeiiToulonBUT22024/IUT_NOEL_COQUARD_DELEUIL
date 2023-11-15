using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotInterface_COQUARD_NOEL
{
    public class Robot
    {
        public Queue<byte> byteListReceived = new Queue<byte>();
        public string receivedText = "";
        public float distanceTelemetreDroit;
        public float distanceTelemetreCentre;
        public float distanceTelemetreGauche;
        public Robot()
        {
            
        }

    }
}