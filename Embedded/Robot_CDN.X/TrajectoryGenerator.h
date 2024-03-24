#ifndef TRAJECTORYGENERATOR_H
#define TRAJECTORYGENERATOR_H

#include <math.h>

#define GHOST_DATA 0x0089

// Parametres de trajectoire
#define MAX_LINEAR_SPEED 1 // m/s
#define MAX_LINEAR_ACCEL 0.5 // m/s^2
#define MAX_ANGULAR_SPEED 0.5 * M_PI * 1.0 // rad/s
#define MAX_ANGULAR_ACCEL 1 * M_PI * 1.0 // rad/s^2
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