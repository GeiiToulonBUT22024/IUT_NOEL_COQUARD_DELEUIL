#ifndef TRAJECTORYGENERATOR_H
#define TRAJECTORYGENERATOR_H

#include <math.h>

// Parametres de trajectoire
#define MAX_LINEAR_SPEED 1 // m/s
#define MAX_LINEAR_ACCEL 0.5 // m/s^2
#define MAX_ANGULAR_SPEED 0.5 * M_PI * 1.0 // rad/s
#define MAX_ANGULAR_ACCEL 1 * M_PI * 1.0 // rad/s^2
#define ANGLE_TOLERANCE 0.05 // radians
#define DISTANCE_TOLERANCE 0.02 // metres

// Position et vitesse du Ghost
typedef struct {
    double x;
    double y;
    double theta;
    double linearSpeed;
    double angularSpeed;
    double targetX;
    double targetY;
    TrajectoryState state;
    
} GhostPosition;

// Etat de controle de la trajectoire
typedef enum {
    IDLE,
    ROTATING,
    ADVANCING
} TrajectoryState;

extern volatile GhostPosition ghostposition;

void InitTrajectoryGenerator(void);
void CalculateGhostPosition(double targetX, double targetY);
void UpdateGhostPosition(void);
void UpdateTrajectory(double currentTime);
void RotateTowardsTarget(double currentTime);
void AdvanceTowardsTarget(double currentTime);

#endif // TRAJECTORYGENERATOR_H