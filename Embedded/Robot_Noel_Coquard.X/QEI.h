/* 
 * File:   QEI.h
 * Author: Table2
 *
 * Created on 29 janvier 2024, 14:35
 */

#ifndef QEI_H
#define	QEI_H

#define DISTROUES 281.2
#define FREQ_ECH_QEI 250

void InitQEI1();
void InitQEI2();
void QEIUpdateData();
void SendPositionData();


#endif	/* QEI_H */

