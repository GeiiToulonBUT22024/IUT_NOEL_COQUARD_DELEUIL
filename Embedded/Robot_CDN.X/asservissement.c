
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


/*
void UpdateAsservissement() 
{
    robotState.PidLin.erreur = ... ; 
    robotState.PidAng.erreur = ... ; 
            
    robotState.xCorrectionVitessePourcent = Correcteur(&robotState.PidLin, robotState.PidAng.erreur);

    robotState.thetaCorrectionVitessePourcent = ... ;

    PWMSetSpeedConsignePolaire(robotState.xCorrectionVitessePourcent, robotState.thetaCorrectionVitessePourcent);   
}
*/