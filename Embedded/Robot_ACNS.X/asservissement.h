#ifndef PID_H
#define	PID_H

typedef struct _PidCorrector
{
    float Kp;
    float Ki;
    float Kd;
    float erreurProportionelleMax;
    float erreurIntegraleMax;
    float erreurDeriveeMax;
    float erreurIntegrale;
    float epsilon_1;
    float erreur;
    //For Debug only
    float corrP;
    float corrI;
    float corrD;
}PidCorrector;

void sendPID(int codeFunction);
void sendAsserv(int codeFunction);
void SetupPidAsservissement(volatile PidCorrector* PidCorr, float Kp, float Ki, float Kd, float proportionelleMax, float integralMax, float deriveeMax);
void UpdateAsservissement();

#endif	/* PID_H */

