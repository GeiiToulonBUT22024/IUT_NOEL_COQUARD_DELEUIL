
#include "asservissement.h"
#include "Utilities.h"
#include "QEI.h"
#include "Robot.h"
#include "PWM.h"
#include "UART_Protocol.h"
#include "TrajectoryGenerator.h"


void SetupPidAsservissement(volatile PidCorrector *PidCorr, double Kp, double Ki, double Kd, double Pmax, double Imax, double Dmax) {
    PidCorr->Kp = Kp;
    PidCorr->erreurPmax = Pmax;
    PidCorr->Ki = Ki;
    PidCorr->erreurImax = Imax;
    PidCorr->Kd = Kd;
    PidCorr->erreurDmax = Dmax;
}

double Correcteur(volatile PidCorrector *PidCorr, double erreur) {
    PidCorr->erreur = erreur;

    PidCorr->erreurP = LimitToInterval(erreur, -PidCorr->erreurPmax / PidCorr->Kp, PidCorr->erreurPmax / PidCorr->Kp);
    PidCorr->corrP = PidCorr->Kp * PidCorr->erreurP;

    PidCorr->erreurI += erreur / FREQ_ECH_QEI;
    PidCorr->erreurI = LimitToInterval(PidCorr->erreurI, -PidCorr->erreurImax / PidCorr->Ki, PidCorr->erreurImax / PidCorr->Ki);
    PidCorr->corrI = PidCorr->Ki * PidCorr->erreurI;

    PidCorr->erreurD = (erreur - PidCorr->erreur_1) * FREQ_ECH_QEI;
    double erreurDlim = LimitToInterval(PidCorr->erreurD, -PidCorr->erreurDmax / PidCorr->Kd, PidCorr->erreurDmax / PidCorr->Kd);
    PidCorr->erreur_1 = erreur;
    PidCorr->corrD = erreurDlim * PidCorr->Kd;

    return PidCorr->corrP + PidCorr->corrI + PidCorr->corrD;
}

void UpdateAsservissement() {
    robotState.PidLin.erreur = robotState.consigneLin - robotState.vitesseLineaireFromOdometry;
    
    robotState.PidAng.erreur = robotState.consigneAng - robotState.vitesseAngulaireFromOdometry; 

    robotState.CorrectionVitesseLineaire = Correcteur(&robotState.PidLin, robotState.PidLin.erreur);
    robotState.CorrectionVitesseAngulaire = Correcteur(&robotState.PidAng, robotState.PidAng.erreur);
    PWMSetSpeedConsignePolaire(robotState.CorrectionVitesseLineaire, robotState.CorrectionVitesseAngulaire);
}

void SendAsservData(volatile PidCorrector *PidCorr, unsigned char PidChoice) {
    unsigned char asservPayload[33];
    asservPayload[0] = PidChoice;
    getBytesFromFloat(asservPayload, 1, (float) (robotState.consigneLin));
    getBytesFromFloat(asservPayload, 5, (float) (robotState.consigneAng));
    getBytesFromFloat(asservPayload, 9, (float) (PidCorr->erreur));
    getBytesFromFloat(asservPayload, 13, (float) (PidCorr->corrP));
    getBytesFromFloat(asservPayload, 17, (float) (PidCorr->corrI));
    getBytesFromFloat(asservPayload, 21, (float) (PidCorr->corrD));
    getBytesFromFloat(asservPayload, 25, (float) (robotState.CorrectionVitesseLineaire));
    getBytesFromFloat(asservPayload, 29, (float) (robotState.CorrectionVitesseAngulaire));
    UartEncodeAndSendMessage(ASSERV_DATA, 33, asservPayload);
}

void SendPidData(volatile PidCorrector *PidCorr, unsigned char PidChoice) {
    unsigned char pidPayload[25];
    pidPayload[0] = PidChoice;
    getBytesFromFloat(pidPayload, 1, (float) (PidCorr ->Kp));
    getBytesFromFloat(pidPayload, 5, (float) (PidCorr->Ki));
    getBytesFromFloat(pidPayload, 9, (float) (PidCorr->Kd));
    getBytesFromFloat(pidPayload, 13, (float) (PidCorr->erreurPmax));
    getBytesFromFloat(pidPayload, 17, (float) (PidCorr->erreurImax));
    getBytesFromFloat(pidPayload, 21, (float) (PidCorr->erreurDmax));
    UartEncodeAndSendMessage(PID_DATA, 25, pidPayload);
}