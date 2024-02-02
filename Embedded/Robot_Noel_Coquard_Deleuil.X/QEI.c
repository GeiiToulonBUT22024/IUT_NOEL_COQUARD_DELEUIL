#include <xc.h>
#include "IO.h"
#include "main.h"
#include "QEI.h"
#include "Utilities.h"
#include "math.h"
#include "Robot.h"
#include "timer.h"

#define POSITION_DATA 0x0061

float QeiDroitPosition_T_1;
float QeiDroitPosition;
float QeiGauchePosition_T_1;
float QeiGauchePosition;
float delta_d;
float delta_g;

const float CONVERSION_MM = 0.01620; 
const float INV_FREQ_ECH_QEI = 1.0 / FREQ_ECH_QEI; 

void InitQEI1() {
    QEI1IOCbits.SWPAB = 1; //QEAx and QEBx are swapped
    QEI1GECL = 0xFFFF;
    QEI1GECH = 0xFFFF;
    QEI1CONbits.QEIEN = 1; // Enable QEI Module
}

void InitQEI2() {
    QEI2IOCbits.SWPAB = 1; //QEAx and QEBx are not swapped
    QEI2GECL = 0xFFFF;
    QEI2GECH = 0xFFFF;
    QEI2CONbits.QEIEN = 1; // Enable QEI Module
}

void QEIUpdateData() {
    // Sauvegarde des anciennes valeurs
    QeiDroitPosition_T_1 = QeiDroitPosition;
    QeiGauchePosition_T_1 = QeiGauchePosition;

    // Actualisation des valeurs des positions
    long QEI1RawValue = POS1CNTL + ((long) POS1HLD << 16);
    long QEI2RawValue = POS2CNTL + ((long) POS2HLD << 16);

    // Conversion en mm
    QeiDroitPosition = CONVERSION_MM * QEI1RawValue;
    QeiGauchePosition = -CONVERSION_MM * QEI2RawValue;

    // Calcul des deltas de position
    delta_d = QeiDroitPosition - QeiDroitPosition_T_1;
    delta_g = QeiGauchePosition - QeiGauchePosition_T_1;

    // Calcul des vitesses
    robotState.vitesseDroitFromOdometry = delta_d * INV_FREQ_ECH_QEI;
    robotState.vitesseGaucheFromOdometry = delta_g * INV_FREQ_ECH_QEI;
    robotState.vitesseLineaireFromOdometry = (robotState.vitesseDroitFromOdometry + robotState.vitesseGaucheFromOdometry) / 2;
    robotState.vitesseAngulaireFromOdometry = (robotState.vitesseDroitFromOdometry - robotState.vitesseGaucheFromOdometry) / DISTROUES;

    // Mise à jour du positionnement terrain à t-1
    robotState.xPosFromOdometry_1 = robotState.xPosFromOdometry;
    robotState.yPosFromOdometry_1 = robotState.yPosFromOdometry;
    robotState.angleRadianFromOdometry_1 = robotState.angleRadianFromOdometry;

    // Calcul des positions dans le référentiel du terrain
    robotState.angleRadianFromOdometry += robotState.vitesseAngulaireFromOdometry * INV_FREQ_ECH_QEI;
    if (robotState.angleRadianFromOdometry > PI)
        robotState.angleRadianFromOdometry -= 2 * PI;
    else if (robotState.angleRadianFromOdometry < -PI)
        robotState.angleRadianFromOdometry += 2 * PI;

    float cosAngle = cos(robotState.angleRadianFromOdometry);
    float sinAngle = sin(robotState.angleRadianFromOdometry);
    robotState.xPosFromOdometry = robotState.xPosFromOdometry_1 + robotState.vitesseLineaireFromOdometry * cosAngle * INV_FREQ_ECH_QEI;
    robotState.yPosFromOdometry = robotState.yPosFromOdometry_1 + robotState.vitesseLineaireFromOdometry * sinAngle * INV_FREQ_ECH_QEI;
}

void SendPositionData() {
    unsigned char positionPayload[24];
    getBytesFromInt32(positionPayload, 0, timestamp);
    getBytesFromFloat(positionPayload, 4, robotState.xPosFromOdometry);
    getBytesFromFloat(positionPayload, 8, robotState.yPosFromOdometry);
    getBytesFromFloat(positionPayload, 12, robotState.angleRadianFromOdometry);
    getBytesFromFloat(positionPayload, 16, robotState.vitesseLineaireFromOdometry);
    getBytesFromFloat(positionPayload, 20, robotState.vitesseAngulaireFromOdometry);
    UartEncodeAndSendMessage(POSITION_DATA, 24, positionPayload);
}
