#include "asservissement.h"

#ifndef ROBOT_H
#define ROBOT_H

typedef struct robotStateBITS
{

    union
    {

        struct
        {
            float vitesseGaucheConsigne;
            float vitesseGaucheCommandeCourante;
            float vitesseDroiteConsigne;
            float vitesseDroiteCommandeCourante;
            float acceleration;

            float distanceTelemetreGauche;
            float distanceTelemetreCentre;
            float distanceTelemetreDroit;
            float distanceTelemetreMelanchon;
            float distanceTelemetreLePen;

            double vitesseDroitFromOdometry;
            double vitesseGaucheFromOdometry;
            double vitesseLineaireFromOdometry;
            double vitesseAngulaireFromOdometry;
            double angleRadianFromOdometry;
            double xPosFromOdometry_1;
            double xPosFromOdometry;
            double yPosFromOdometry_1;
            double yPosFromOdometry;
            double angleRadianFromOdometry_1;

            double consigneLin;
            double consigneAng;

            double ghostX;
            double ghostY;

            PidCorrector PidLin;
            PidCorrector PidAng;
            PidCorrector PidPos;

            double CorrectionVitesseLineaire;
            double CorrectionVitesseAngulaire;
            double CorrectionPosition;

            int stop;
            int mode;
        };
    };
} ROBOT_STATE_BITS;

extern volatile ROBOT_STATE_BITS robotState;

#endif