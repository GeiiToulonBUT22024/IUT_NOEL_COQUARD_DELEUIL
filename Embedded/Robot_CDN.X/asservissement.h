#ifndef ASSERVISSEMENT_H
#define	ASSERVISSEMENT_H

typedef struct _PidCorrector {
    double Kp;
    double Ki;
    double Kd;

    double erreur;
    double epsilon;


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

#endif