
#include "asservissement.h"
#include "Utilities.h"
#include "QEI.h"
#include "Robot.h"

void SetupPidAsservissement(volatile PidCorrector *PidCorr, double Kp, double Ki, double Kd, double Pmax, double Imax, double Dmax)
{
    PidCorr->Kp = Kp;
    PidCorr->erreurPmax = Pmax; // On limite la correction due au Kp
    PidCorr->Ki = Ki;
    PidCorr->erreurImax = Imax; // On limite la correction due au Ki
    PidCorr->Kd = Kd;
    PidCorr->erreurDmax = Dmax; // On limite la correction due au Kd
}

double Correcteur(volatile PidCorrector *PidCorr, double erreur)
{
    PidCorr->erreur = erreur;

    PidCorr->erreurP = LimitToInterval(erreur, -PidCorr->erreurPmax / PidCorr->Kp, PidCorr->erreurPmax / PidCorr->Kp);
    PidCorr->corrP = PidCorr->Kp * PidCorr->erreurP;

    PidCorr->erreurI = LimitToInterval((PidCorr->erreurI + erreur / FREQ_ECH_QEI), -PidCorr->erreurImax / PidCorr->Ki, PidCorr->erreurImax / PidCorr->Ki);
    PidCorr->corrI = PidCorr->Ki * PidCorr->erreurI;
    
    PidCorr->erreurD = (erreur - PidCorr->epsilon) * FREQ_ECH_QEI;
    double erreurDlim = LimitToInterval(PidCorr->erreurD, -PidCorr->erreurDmax / PidCorr->Kd, PidCorr->erreurDmax / PidCorr->Kd);
    PidCorr->epsilon = erreur;
    PidCorr->corrD = erreurDlim * PidCorr->Kd;

    return PidCorr->corrP + PidCorr->corrI + PidCorr->corrD;
}


void UpdateAsservissement() 
{
    robotState.PidLin.erreur = (robotState.consigne - robotState.vitesseLineaireFromOdometry);     
    robotState.PidAng.erreur = (robotState.consigneAng - robotState.vitesseAngulaireFromOdometry); 

    robotState.xCorrectionVitessePourcent = Correcteur(&robotState.PidLin, robotState.PidAng.erreur);
    robotState.thetaCorrectionVitessePourcent = Correcteur(&robotState.PidAng, robotState.PidLin.erreur) ;
    PWMSetSpeedConsignePolaire(robotState.xCorrectionVitessePourcent, robotState.thetaCorrectionVitessePourcent);   
}

void SendAsservData(volatile PidCorrector *PidCorr, unsigned char PidChoice)
{
    unsigned char positionPayload[25];
    positionPayload[0] = PidChoice;
    getBytesFromFloat(positionPayload, 1,(float) (robotState.consigne));
    getBytesFromFloat(positionPayload, 5, (float)(robotState.consigneAng));
    getBytesFromFloat(positionPayload, 9, (float)(PidCorr->erreur));
    getBytesFromFloat(positionPayload, 13, (float)(PidCorr->corrP));
    getBytesFromFloat(positionPayload, 17, (float)(PidCorr->corrI));
    getBytesFromFloat(positionPayload, 21, (float)(PidCorr->corrD));
    UartEncodeAndSendMessage(ASSERV_DATA, 25, positionPayload);
}


void SendPidData(volatile PidCorrector *PidCorr, unsigned char PidChoice)
{
    unsigned char positionPayload[25];
    positionPayload[0] = PidChoice;
    getBytesFromFloat(positionPayload, 1, (float)(PidCorr ->Kp));
    getBytesFromFloat(positionPayload, 5, (float)(PidCorr->Ki));
    getBytesFromFloat(positionPayload, 9, (float)(PidCorr->Kd));
    getBytesFromFloat(positionPayload, 13, (float)(PidCorr->erreurPmax));
    getBytesFromFloat(positionPayload, 17, (float)(PidCorr->erreurImax));
    getBytesFromFloat(positionPayload, 21, (float)(PidCorr->erreurDmax));
    UartEncodeAndSendMessage(PID_DATA, 25, positionPayload);
}

