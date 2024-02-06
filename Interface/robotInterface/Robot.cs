﻿using System;
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

        public float timestamp;
        public float positionXOdo;
        public float positionYOdo;
        public float angle;
        public float vitLin;
        public float vitAng;


        public Queue<string> stringListReceived = new Queue<string>();


        public Robot()
        {

        }
    }
}