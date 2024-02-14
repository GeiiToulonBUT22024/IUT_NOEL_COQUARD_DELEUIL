#ifndef ASSERVISSEMENT_H
#define	ASSERVISSEMENT_H

#define ASSERV_DATA 0x0070
#define PID_DATA 0x0072

#define PID_LIN 0
#define PID_ANG 1


typedef struct _PidCorrector {
    double Kp;
    double Ki;
    double Kd;

    double erreur;
    double erreur_1;

    double erreurP;
    double erreurI;
    double erreurD;

    double erreurPmax;
    double erreurImax;
    double erreurDmax;

    //For Debug only
    double corrP;
    double corrI;
    double corrD;
   
} PidCorrector;

void SetupPidAsservissement(volatile PidCorrector *PidCorr, double Kp, double Ki, double Kd, double Pmax, double Imax, double Dmax);
double Correcteur(volatile PidCorrector *PidCorr, double erreur);
void UpdateAsservissement();
void SendPidData(volatile PidCorrector *PidCorr, unsigned char PidChoice);
void SendAsservData(volatile PidCorrector *PidCorr, unsigned char PidChoice);

#endif