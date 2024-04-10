#ifndef TRAJECTORYGENERATOR_H
#define TRAJECTORYGENERATOR_H

#include <math.h>

#define GHOST_DATA 0x0089

// Parametres de trajectoire
//#define MAX_LINEAR_SPEED 0.6 // m/s
//#define MAX_LINEAR_ACCEL 0.2 // m/s^2

#define ANGLE_TOLERANCE 0.05 // radians
#define DISTANCE_TOLERANCE 0.1 // metres


// Etat de controle de la trajectoire
typedef enum {
    IDLE,
    ROTATING,
    ADVANCING
} TrajectoryState;

// Position et vitesse du Ghost
typedef struct {
    double x;
    double y;
    double theta;
    double linearSpeed;
    double angularSpeed;
    double targetX;
    double targetY;
    double angleToTarget;
    double distanceToTarget;

    TrajectoryState state;
    
} GhostPosition;


extern volatile GhostPosition ghostposition;

void InitTrajectoryGenerator(void);
void UpdateTrajectory();
void RotateTowardsTarget(double currentTime);
void AdvanceTowardsTarget(double currentTime);
void SendGhostData();

#endif // TRAJECTORYGENERATOR_H