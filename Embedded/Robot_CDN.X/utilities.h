#ifndef UTILITIES_H
#define	UTILITIES_H

# define PI 3.141592653589793

double DistanceProjete(double ptX, double ptY, double ptSeg1X, double ptSeg1Y, double ptSeg2X, double ptSeg2Y);
double ModuloByAngle(double angleToCenterAround, double angleToCorrect);
double Abs(double value);
double Max(double value, double value2);
double Min(double value,double value2);
double LimitToInterval(double value, double min, double max);
double Modulo2PIAngleRadian(double angleRadian) ;
float getFloat(unsigned char *p, int index);
double getDouble(unsigned char *p, int index);
void getBytesFromFloat(unsigned char *p, int index, float f);
void getBytesFromInt32(unsigned char *p, int index, long in);
void getBytesFromDouble(unsigned char *p, int index, double d);

#endif /*UTILITIES_H*/

