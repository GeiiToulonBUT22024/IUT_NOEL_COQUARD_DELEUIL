#include "asservissement.h"
#include "Utilities.h"
#include "QEI.h"
#include "Robot.h"
#include "PWM.h"
#include "UART_Protocol.h"
#include "TrajectoryGenerator.h"

// Configuration du correcteur PID pour la vitesse
void SetupPidAsservissement(volatile PidCorrector *PidCorr, double Kp, double Ki, double Kd, double Pmax, double Imax, double Dmax)
{
    PidCorr->Kp = Kp;
    PidCorr->erreurPmax = Pmax;
    PidCorr->Ki = Ki;
    PidCorr->erreurImax = Imax;
    PidCorr->Kd = Kd;
    PidCorr->erreurDmax = Dmax;
}

// Configuration du correcteur PD pour la position
void SetupPdAsservissement(volatile PidCorrector *PidCorr, double Kp, double Kd, double Pmax, double Dmax)
{
    PidCorr->Kp = Kp;
    PidCorr->erreurPmax = Pmax;
    PidCorr->Ki = 0; // Ki est mis a 0
    PidCorr->Kd = Kd;
    PidCorr->erreurDmax = Dmax;
}

// Correcteur PID pour la vitesse
double Correcteur(volatile PidCorrector *PidCorr, double erreur)
{
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

// Correcteur PD pour la position
double CorrecteurPD(volatile PidCorrector *PidCorr, double erreur)
{
    PidCorr->erreur = erreur;

    PidCorr->erreurP = LimitToInterval(erreur, -PidCorr->erreurPmax / PidCorr->Kp, PidCorr->erreurPmax / PidCorr->Kp);
    PidCorr->corrP = PidCorr->Kp * PidCorr->erreurP;

    PidCorr->erreurD = (erreur - PidCorr->erreur_1) * FREQ_ECH_QEI;
    double erreurDlim = LimitToInterval(PidCorr->erreurD, -PidCorr->erreurDmax / PidCorr->Kd, PidCorr->erreurDmax / PidCorr->Kd);
    PidCorr->erreur_1 = erreur;
    PidCorr->corrD = erreurDlim * PidCorr->Kd;

    return PidCorr->corrP + PidCorr->corrD;
}

void UpdateAsservissement()
{
    // Calcul et application du correcteur PID pour la vitesse lineaire
    //robotState.PidLin.erreur = robotState.consigneLin - robotState.vitesseLineaireFromOdometry;
    robotState.PidLin.erreur = 0;
    
    robotState.CorrectionVitesseLineaire = Correcteur(&robotState.PidLin, robotState.PidLin.erreur);

    // Calcul et application du correcteur PID pour la vitesse angulaire
    robotState.PidAng.erreur = robotState.consigneAng - robotState.vitesseAngulaireFromOdometry;
    robotState.CorrectionVitesseAngulaire = Correcteur(&robotState.PidAng, robotState.PidAng.erreur);

    // Calcul de l'erreur de position en fonction de la distance entre la position actuelle et la position cible (ghost)
    double erreurPosition = sqrt(pow(robotState.ghostX - robotState.xPosFromOdometry, 2) + pow(robotState.ghostY - robotState.yPosFromOdometry, 2));


    // Application du correcteur PD pour ajuster la consigne de vitesse lineaire en fonction de l'erreur de position
    // -> Choisir la version la plus appropriee, je ne sais pas ce qui fonctionne le mieux en vrai, il faudra tester les gars
    
    /* Version 1:
    - Ajout de la correction a la consigne actuelle
    - Permet une correction progressive, mais peut entrainer une derive si accumulee trop longtemps */
    robotState.CorrectionPosition = CorrecteurPD(&robotState.PidPos, erreurPosition);
    robotState.consigneLin += robotState.CorrectionPosition;

    /* Version 2:
    - Remplacement direct de la consigne par la correction calculee
    - Offre un controle plus stable et rapide sans accumulation indesirable */
    robotState.consigneLin = CorrecteurPD(&robotState.PidPos, erreurPosition);


    // Application des consignes de vitesse calculees aux moteurs
    PWMSetSpeedConsignePolaire(robotState.CorrectionVitesseLineaire, robotState.CorrectionVitesseAngulaire);
}

void SendAsservData(volatile PidCorrector *PidCorr, unsigned char PidChoice)
{
    unsigned char asservPayload[33];
    asservPayload[0] = PidChoice;
    getBytesFromFloat(asservPayload, 1, (float) (robotState.consigneLin));
    getBytesFromFloat(asservPayload, 5, (float) (robotState.consigneAng));
    getBytesFromFloat(asservPayload, 9, (float) (PidCorr->erreur));
    getBytesFromFloat(asservPayload, 13, (float) (PidCorr->corrP));
    getBytesFromFloat(asservPayload, 17, (float) (PidCorr->corrI)); // corrI = 0 pour PD
    getBytesFromFloat(asservPayload, 21, (float) (PidCorr->corrD));
    getBytesFromFloat(asservPayload, 25, (float) (robotState.CorrectionVitesseLineaire));
    getBytesFromFloat(asservPayload, 29, (float) (robotState.CorrectionVitesseAngulaire));
    UartEncodeAndSendMessage(ASSERV_DATA, 33, asservPayload);
}

void SendPidData(volatile PidCorrector *PidCorr, unsigned char PidChoice)
{
    unsigned char pidPayload[25];
    pidPayload[0] = PidChoice;
    getBytesFromFloat(pidPayload, 1, (float) (PidCorr->Kp));
    getBytesFromFloat(pidPayload, 5, (float) (PidCorr->Ki)); // Ki = 0 pour PD
    getBytesFromFloat(pidPayload, 9, (float) (PidCorr->Kd));
    getBytesFromFloat(pidPayload, 13, (float) (PidCorr->erreurPmax));
    getBytesFromFloat(pidPayload, 17, (float) (PidCorr->erreurImax)); // erreurImax = 0 pour PD
    getBytesFromFloat(pidPayload, 21, (float) (PidCorr->erreurDmax));
    UartEncodeAndSendMessage(PID_DATA, 25, pidPayload);
}