#ifndef TRAJECTORYGENERATOR_H
#define TRAJECTORYGENERATOR_H

#include <math.h>

#define GHOST_DATA 0x0089

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
} GhostPosition_t;

extern volatile GhostPosition_t ghostPosition;

void InitTrajectoryGenerator(void);
void UpdateTrajectory();
void RotateTowardsTarget(double currentTime);
void AdvanceTowardsTarget(double currentTime);
void SendGhostData();

#endif // TRAJECTORYGENERATOR_H