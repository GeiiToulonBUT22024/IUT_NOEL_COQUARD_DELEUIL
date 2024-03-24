#include "Utilities.h"
#include "math.h"
#include <xc.h>


// A et B deux point sur une meme droite (generalement AB est unitaire). C un point a projetter sur cette droite. 
// La fonction renvoie la distance entre A et le projette de ce point (cette distance peut etre negative en fonction de l'orientation de AB)
double DistanceProjete(double Ax, double Ay, double Bx, double By, double Cx, double Cy)
{
    // Calcul du vecteur AB (rendu unitaire)
    double vect1x = Bx - Ax; 
    double vect1y = By - Ay;
    double norm = sqrt(vect1x * vect1x + vect1y * vect1y);
    vect1x = vect1x / norm;
    vect1y = vect1y / norm;
            
    // Calcul du vecteur AC 
    double vect2x = Cx - Ax;
    double vect2y = Cy - Ay;

    double dot = vect1x * vect2x + vect1y * vect2y ; // AD = AB.AC = |AB| * |AC| * cos(AB, AC)
    return dot;
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