using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace robotInterface
{
    public class TrajectoryManager
    {

        public TrajectoryGenerator Generator { get; private set; } = new TrajectoryGenerator();

        public readonly Dictionary<string, (float X, float Y)> pointsList = new Dictionary<string, (float X, float Y)>
        {
            {"Centre", (150, 100)},
            {"Zone Haut Gauche", (22, 178)},
            {"Zone Milieu Gauche", (22, 100)},
            {"Zone Bas Gauche", (22, 22)},
            {"Zone Haut Droit", (278, 178)},
            {"Zone Milieu Droit", (278, 100)},
            {"Zone Bas Droit", (278, 22)},
            {"Balise Haut Gauche", (75, 150)},
            {"Balise Bas Gauche", (75, 50)},
            {"Balise Haut Droit", (225, 150)},
            {"Balise Bas Droit", (225, 50)}
        };

        public static class Constants
        {
            public const double MAX_LINEAR_SPEED = 20.0; // m/s
            public const double MAX_LINEAR_ACCEL = 10.0; // m/s^2
            public const double MAX_ANGULAR_SPEED = Math.PI * 0.5; // rad/s
            public const double MAX_ANGULAR_ACCEL = Math.PI; // rad/s^2
            public const double ANGLE_TOLERANCE = 0.03; // radians
            public const double DISTANCE_TOLERANCE = 0.1; // meters
        }

        public enum TrajectoryState
        {
            Idle,
            Rotating,
            Advancing
        }

        public class GhostPosition
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Theta { get; set; }
            public double LinearSpeed { get; set; }
            public double AngularSpeed { get; set; }
            public double TargetX { get; set; }
            public double TargetY { get; set; }
            public double AngleToTarget { get; set; }
            public double DistanceToTarget { get; set; }
            public TrajectoryState State { get; set; } = TrajectoryState.Idle;
        }

        public class TrajectoryGenerator
        {
            public GhostPosition GhostPosition { get; private set; } = new GhostPosition();
            private DateTime lastUpdateTime = DateTime.Now;

            public TrajectoryGenerator()
            {
                GhostPosition = new GhostPosition
                {
                    X = 22,
                    Y = 22,
                    Theta = 0,
                    LinearSpeed = 0,
                    AngularSpeed = 0,
                    TargetX = 22,
                    TargetY = 22,
                    AngleToTarget = 0,
                    DistanceToTarget = 0,
                    State = TrajectoryState.Idle
                };
            }

            public void UpdateTrajectory()
            {
                var currentTime = DateTime.Now;
                var deltaTime = (currentTime - lastUpdateTime).TotalSeconds;

                switch (GhostPosition.State)
                {
                    case TrajectoryState.Idle:
                        HandleIdleState();
                        break;
                    case TrajectoryState.Rotating:
                        RotateTowardsTarget(deltaTime);
                        break;
                    case TrajectoryState.Advancing:
                        AdvanceTowardsTarget(deltaTime);
                        break;
                }

                lastUpdateTime = currentTime;
            }

            private void HandleIdleState()
            {
                GhostPosition.LinearSpeed = 0.0;
                GhostPosition.AngularSpeed = 0.0;

                if (GhostPosition.TargetX != 0.0 || GhostPosition.TargetY != 0.0)
                {
                    double angleToTarget = Math.Atan2(GhostPosition.TargetY - GhostPosition.Y, GhostPosition.TargetX - GhostPosition.X);
                    double angleDifference = ModuloByAngle(GhostPosition.Theta, angleToTarget - GhostPosition.Theta);

                    if (Math.Abs(angleDifference) > Constants.ANGLE_TOLERANCE)
                    {
                        GhostPosition.State = TrajectoryState.Rotating;
                    }
                    else
                    {
                        GhostPosition.State = TrajectoryState.Advancing;
                    }
                }
            }

            private double DistanceProjete(double Ax, double Ay, double Bx, double By, double Cx, double Cy)
            {
                double vect1x = Bx - Ax;
                double vect1y = By - Ay;
                double norm = Math.Sqrt(vect1x * vect1x + vect1y * vect1y);
                vect1x /= norm;
                vect1y /= norm;

                double vect2x = Cx - Ax;
                double vect2y = Cy - Ay;

                return vect1x * vect2x + vect1y * vect2y;
            }

            private double ModuloByAngle(double baseAngle, double angleDifference)
            {
                double modAngle = angleDifference % (2 * Math.PI);
                //modAngle < 0 ? modAngle + 2 * Math.PI : modAngle;

                if (modAngle > Math.PI)
                {
                    modAngle = -(modAngle - Math.PI);
                }

                return modAngle;

            }

            //public static double ModuloByAngle(double angleToCenterAround, double angleToCorrect)
            //{
            //    // On corrige l'angle obtenu pour le moduloter autour de l'angle Kalman
            //    int decalageNbTours = (int)Math.Round((angleToCorrect - angleToCenterAround) / (2 * Math.PI));
            //    double thetaDest = angleToCorrect - decalageNbTours * 2 * Math.PI;

            //    return thetaDest;
            //}



            private void RotateTowardsTarget(double deltaTime)
            {
                double thetaWaypoint = Math.Atan2(GhostPosition.TargetY - GhostPosition.Y, GhostPosition.TargetX - GhostPosition.X);
                double thetaRestant = ModuloByAngle(GhostPosition.Theta, thetaWaypoint - GhostPosition.Theta);
                double thetaArret = Math.Pow(GhostPosition.AngularSpeed, 2) / (2 * Constants.MAX_ANGULAR_ACCEL);

                bool isDirectionPositive = thetaRestant > 0;

                if (Math.Abs(thetaRestant) < Constants.ANGLE_TOLERANCE)
                {
                    GhostPosition.State = TrajectoryState.Advancing;
                    GhostPosition.AngularSpeed = 0;
                    return;
                }

                bool shouldAccelerate = false;

                if (isDirectionPositive)
                {
                    shouldAccelerate = GhostPosition.AngularSpeed < Constants.MAX_ANGULAR_SPEED && thetaRestant > thetaArret;
                }
                else
                {
                    shouldAccelerate = GhostPosition.AngularSpeed > -Constants.MAX_ANGULAR_SPEED && Math.Abs(thetaRestant) > thetaArret;
                }

                if (shouldAccelerate)
                {
                    GhostPosition.AngularSpeed += (isDirectionPositive ? 1 : -1) * Constants.MAX_ANGULAR_ACCEL * deltaTime;
                }
                else if (Math.Abs(thetaRestant) <= thetaArret || (!isDirectionPositive && GhostPosition.AngularSpeed > 0) || (isDirectionPositive && GhostPosition.AngularSpeed < 0))
                {
                    GhostPosition.AngularSpeed -= (isDirectionPositive ? 1 : -1) * Constants.MAX_ANGULAR_ACCEL * deltaTime;
                }

                if (GhostPosition.AngularSpeed > Constants.MAX_ANGULAR_SPEED) GhostPosition.AngularSpeed = Constants.MAX_ANGULAR_SPEED;

                GhostPosition.AngularSpeed = Math.Min(Math.Max(GhostPosition.AngularSpeed, -Constants.MAX_ANGULAR_SPEED), Constants.MAX_ANGULAR_SPEED);
                GhostPosition.Theta += GhostPosition.AngularSpeed * deltaTime;
                GhostPosition.Theta = ModuloByAngle(0, GhostPosition.Theta);

                GhostPosition.AngleToTarget = thetaRestant;
            }


            private void AdvanceTowardsTarget(double deltaTime)
            {
                double distance = DistanceProjete(GhostPosition.X, GhostPosition.Y,
                                                   GhostPosition.X + Math.Cos(GhostPosition.Theta),
                                                   GhostPosition.Y + Math.Sin(GhostPosition.Theta),
                                                   GhostPosition.TargetX, GhostPosition.TargetY);

                if (distance < Constants.DISTANCE_TOLERANCE)
                {
                    GhostPosition.State = TrajectoryState.Idle;
                    GhostPosition.LinearSpeed = 0.0;
                    GhostPosition.TargetX = GhostPosition.X;
                    GhostPosition.TargetY = GhostPosition.Y;
                    return;
                }

                double accelDistance = (Math.Pow(Constants.MAX_LINEAR_SPEED, 2) - Math.Pow(GhostPosition.LinearSpeed, 2)) / (2 * Constants.MAX_LINEAR_ACCEL);
                double decelDistance = (Math.Pow(GhostPosition.LinearSpeed, 2)) / (2 * Constants.MAX_LINEAR_ACCEL);

                if (distance <= (decelDistance + Constants.DISTANCE_TOLERANCE))
                {
                    GhostPosition.LinearSpeed = Math.Max(0.0, (Constants.MAX_LINEAR_SPEED * distance) / decelDistance);
                }
                else if (distance > decelDistance + accelDistance)
                {
                    GhostPosition.LinearSpeed += Constants.MAX_LINEAR_ACCEL * deltaTime;
                    GhostPosition.LinearSpeed = Math.Min(GhostPosition.LinearSpeed, Constants.MAX_LINEAR_SPEED);
                }
                else
                {
                    double vMedian = Math.Sqrt(Constants.MAX_LINEAR_ACCEL * distance + Math.Pow(GhostPosition.LinearSpeed, 2) / 2);
                    GhostPosition.LinearSpeed += Constants.MAX_LINEAR_ACCEL * deltaTime;
                    GhostPosition.LinearSpeed = Math.Min(GhostPosition.LinearSpeed, vMedian);
                }

                if (GhostPosition.LinearSpeed > Constants.MAX_LINEAR_SPEED) GhostPosition.LinearSpeed = Constants.MAX_LINEAR_SPEED;
                else 
                


                GhostPosition.DistanceToTarget = distance;

                GhostPosition.X += GhostPosition.LinearSpeed * Math.Cos(GhostPosition.Theta) * deltaTime;
                GhostPosition.Y += GhostPosition.LinearSpeed * Math.Sin(GhostPosition.Theta) * deltaTime;
            }
        }
    }
}