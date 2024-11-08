/* 
 * File:   QEI.h
 * Author: TP-EO-5
 *
 * Created on 29 janvier 2024, 13:26
 */

#ifndef QEI_H
#define	QEI_H

void InitQEI1();
void InitQEI2();
void QEIUpdateData();
void SendPositionData();
#define COEF_VITESSE_POURCENT 40
#define DISTROUES 0.216
#define FREQ_ECH_QEI 250
#define POSITION_DATA 0x0062



#endif	/* QEI_H */

