#include "Utilities.h"
#include "math.h"
#include <xc.h>



double DistancePointToSegment(double ptX, double ptY, double ptSeg1X, double ptSeg1Y, double ptSeg2X, double ptSeg2Y)
{
    // Calcul des vecteurs entre le point et le premier point du segment
    double A = ptX - ptSeg1X;
    double B = ptY - ptSeg1Y;

    // Calcul du vecteur directeur du segment
    double C = ptSeg2X - ptSeg1X;
    double D = ptSeg2Y - ptSeg1Y;

    double dot = A * C + B * D; // Calcul du produit scalaire des vecteurs (A,B) et (C,D)
    double len_sq = C * C + D * D; // Calcul du carre de la longueur du segment

    double param = -1; // Calcul du paramètre qui nous indique le point le plus proche sur le segment par rapport au point donne
    if (len_sq != 0) 
        param = dot / len_sq; // Pour eviter la division par zero

    double xx, yy;

    // Si param < 0, cela signifie que le point projete tombe en dehors du segment, plus proche de ptSeg1
    if (param < 0)
    {
        xx = ptSeg1X;
        yy = ptSeg1Y;
    }
    // Si param > 1, le point projete tombe egalement en dehors du segment, mais plus proche de ptSeg2
    else if (param > 1)
    {
        xx = ptSeg2X;
        yy = ptSeg2Y;
    }
    // Si 0 <= param <= 1, le point projete tombe sur le segment
    else
    {
        xx = ptSeg1X + param * C;
        yy = ptSeg1Y + param * D;
    }

    // Calcul de la distance entre le point donne et le point le plus proche sur le segment
    double dx = ptX - xx;
    double dy = ptY - yy;
    return sqrt(dx * dx + dy * dy);
}


double ModuloByAngle(double angleToCenterAround, double angleToCorrect) // Fonction pour normaliser un angle entre -PI et PI
{
    int decalageNbTours = (int) round((angleToCorrect - angleToCenterAround) / (2 * M_PI));
    double thetaDest = angleToCorrect - decalageNbTours * 2 * M_PI;
    return thetaDest;
}

double Abs(double value)
{
    if (value>=0)
        return value;
    else return -value;      
}

double Max(double value, double value2)
{
    if(value>value2)
        return value;
    else return value2;
}

double Min(double value,double value2)
{
    if (value < value2)
        return value;
    else
        return value2;    
}

double LimitToInterval(double value, double min, double max)
{
    if(value < min)
        return min;
    else if(value > max)
        return max;
    else
        return value;
}

double Modulo2PIAngleRadian(double angleRadian) {
    double angleTemp = fmod(angleRadian - PI, 2 * PI) + PI;
    return fmod(angleTemp + PI, 2 * PI) - PI;
}

float getFloat(unsigned char *p, int index)
{
    float *result_ptr = (float*)(p + index);
    //float result = *result_ptr;
    return *result_ptr;
}

double getDouble(unsigned char *p, int index)
{
    double *result_ptr = (double*)(p + index);
    return *result_ptr;
}

void getBytesFromFloat(unsigned char *p, int index, float f)
{
    int i;
    unsigned char *f_ptr = (unsigned char*)&f;
    for (i = 0; i < 4; i++)
        p[index + i] = f_ptr[i];
}

void getBytesFromInt32(unsigned char *p, int index, long in)
{
    int i;
    unsigned char *f_ptr = (unsigned char*)&in;
    for (i = 0; i < 4; i++)
        p[index + i] = f_ptr[3-i];
}

void getBytesFromDouble(unsigned char *p, int index, double d)
{
    int i;
    unsigned char *f_ptr = (unsigned char*)&d;
    for (i = 0; i < 8; i++)
        p[index + i] = f_ptr[i];
}