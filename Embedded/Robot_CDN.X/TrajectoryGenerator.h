#ifndef TRAJECTORYGENERATOR_H
#define TRAJECTORYGENERATOR_H

#include <math.h>

// Parametres de trajectoire
#define MAX_LINEAR_SPEED 0.5 // m/s
#define MAX_LINEAR_ACCEL 0.2 // m/s^2
#define MAX_ANGULAR_SPEED 1.0 // rad/s
#define MAX_ANGULAR_ACCEL 0.5 // rad/s^2
#define ANGLE_TOLERANCE 0.05 // radians
#define DISTANCE_TOLERANCE 0.02 // metres

// Position et vitesse du Ghost
typedef struct {
    double x;
    double y;
    double theta;
    double linearSpeed;
    double angularSpeed;
} GhostPosition;

// Etat de controle de la trajectoire
typedef enum {
    IDLE,
    ROTATING,
    ADVANCING
} TrajectoryState;

// Structure pour controler la trajectoire
typedef struct {
    TrajectoryState state;
    double targetX;
    double targetY;
    double targetTheta;
} TrajectoryControl;

double ModuloByAngle(double angleToCenterAround, double angleToCorrect);
void InitTrajectoryGenerator(void);
void CalculateGhostPosition(double targetX, double targetY, double targetTheta);
void UpdateGhostPosition(void);
void UpdateTrajectory(double currentTime);
void RotateTowardsTarget(double currentTime);
double DistancePointToSegment(double ptX, double ptY, double ptSeg1X, double ptSeg1Y, double ptSeg2X, double ptSeg2Y);
void AdvanceTowardsTarget(double currentTime);

#endif // TRAJECTORYGENERATOR_H