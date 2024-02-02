/* 
 * File:   Toolbox.h
 * Author: GEII Robot
 *
 * Created on 27 septembre 2023, 08:45
 */

#ifndef TOOLBOX_H
#define	TOOLBOX_H

float Abs(float value);
float Max(float value, float value2);
float Min(float value, float value2);
float LimitToInterval(float value, float lowLimit, float highLimit);
float RadianToDegree(float value);
float DegreeToRadian(float value);

#define PI 3.141592653589793

#endif	/* TOOLBOX_H */

