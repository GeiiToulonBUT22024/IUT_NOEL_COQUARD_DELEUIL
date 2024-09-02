using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Utilities;

namespace robotInterface
{
    public class TrajectoryManager
    {
        public TrajectoryGenerator Generator { get; private set; } = new TrajectoryGenerator();

        // Valeurs de constantes adaptées à la simulation
        public static class Constants
        {
            public const double MaxAngularSpeed = 5 * Math.PI;
            public const double AngularAccel = 2 * Math.PI;
            public const double MaxLinearSpeed = 50.0;
            public const double MinMaxLinearSpeed = 5.0;
            public const double LinearAccel = 20.0;
            public const double FreqEchQEI = 100;
        }

        public class GhostPosition
        {
            public double X { get; set; } = 0.0;
            public double Y { get; set; } = 0.0;
            public double Theta { get; set; } = 0.0;
            public double LinearSpeed { get; set; } = 0.0; // (m/s)
            public double AngularSpeed { get; set; } = 0.0; // (rad/s)
            public double TargetX { get; set; } = 0.0;
            public double TargetY { get; set; } = 0.0;
            public double AngleToTarget { get; set; } = 0.0; // (rad)
            public double DistanceToTarget { get; set; } = 0.0; // (m)
        }

        public class TrajectoryGenerator
        {
            // Propriété représentant la position actuelle du ghost
            public GhostPosition GhostPosition { get; private set; } = new GhostPosition();

            // Constructeur initialisant la position du ghost
            public TrajectoryGenerator()
            {
                GhostPosition = new GhostPosition
                {
                    X = 0.0,
                    Y = 0.0,
                    Theta = 0.0,
                    LinearSpeed = 0.0,
                    AngularSpeed = 0.0,
                    TargetX = 0.0,
                    TargetY = 0.0,
                    AngleToTarget = 0.0,
                    DistanceToTarget = 0.0
                };
            }

            public void UpdateTrajectory()
            {
                // Calcul de l'angle vers la cible (radians)
                double targetAngle = Math.Atan2(GhostPosition.TargetY - GhostPosition.Y, GhostPosition.TargetX - GhostPosition.X);

                // Calcul des valeurs nécessaires pour la mise à jour de la vitesse et de la position
                double angleToCover = Toolbox.ModuloByAngle(GhostPosition.Theta, targetAngle) - GhostPosition.Theta;
                double stopAngle = GhostPosition.AngularSpeed * GhostPosition.AngularSpeed / (2 * Constants.AngularAccel);
                double distanceToCover = Math.Sqrt(Math.Pow(GhostPosition.TargetX - GhostPosition.X, 2) + Math.Pow(GhostPosition.TargetY - GhostPosition.Y, 2));
                double stopDistance = GhostPosition.LinearSpeed * GhostPosition.LinearSpeed / (2 * Constants.LinearAccel);
                double maxLinearSpeed = 0.5 * ((Constants.MaxLinearSpeed + Constants.MinMaxLinearSpeed) + (Constants.MaxLinearSpeed - Constants.MinMaxLinearSpeed) * Math.Cos(angleToCover));
                double maxStopRadius = 0.2 * (Constants.MaxLinearSpeed + Constants.MinMaxLinearSpeed) / Constants.MaxAngularSpeed;

                GhostPosition.AngleToTarget = targetAngle;
                GhostPosition.DistanceToTarget = distanceToCover;

                // Gestion de la rotation pour atteindre l'angle cible
                if (angleToCover != 0 && distanceToCover > 0.01)
                {
                    if (angleToCover > 0) // Si l'angle à parcourir est positif
                    {
                        if (angleToCover > stopAngle) // Si l'angle à parcourir est supérieur à l'angle d'arrêt
                        {
                            if (GhostPosition.AngularSpeed >= Constants.MaxAngularSpeed) // Si la vitesse angulaire maximale est atteinte
                            {
                                // Vitesse angulaire maintenue
                            }
                            else
                            {
                                // Accélération avec saturation à la vitesse maximale
                                GhostPosition.AngularSpeed = Math.Min(GhostPosition.AngularSpeed + Constants.AngularAccel / Constants.FreqEchQEI, Constants.MaxAngularSpeed);
                            }
                        }
                        else // Si l'angle à parcourir est inférieur à l'angle d'arrêt
                        {
                            // Décélération pour atteindre l'angle cible
                            GhostPosition.AngularSpeed = Math.Max(GhostPosition.AngularSpeed - Constants.AngularAccel / Constants.FreqEchQEI, 0);
                        }
                    }
                    else // Si l'angle à parcourir est négatif
                    {
                        if (Math.Abs(angleToCover) > stopAngle) // Si l'angle à parcourir est supérieur à l'angle d'arrêt
                        {
                            if (GhostPosition.AngularSpeed <= -Constants.MaxAngularSpeed) // Si la vitesse angulaire maximale négative est atteinte
                            {
                                // Vitesse angulaire maintenue
                            }
                            else
                            {
                                // Accélération avec saturation à la vitesse maximale négative
                                GhostPosition.AngularSpeed = Math.Max(GhostPosition.AngularSpeed - Constants.AngularAccel / Constants.FreqEchQEI, -Constants.MaxAngularSpeed);
                            }
                        }
                        else // Si l'angle à parcourir est inférieur à l'angle d'arrêt
                        {
                            // Décélération pour atteindre l'angle cible
                            GhostPosition.AngularSpeed = Math.Min(GhostPosition.AngularSpeed + Constants.AngularAccel / Constants.FreqEchQEI, 0);
                        }
                    }

                    // Mise à jour de l'orientation du ghost en fonction de la vitesse angulaire calculée
                    GhostPosition.Theta += GhostPosition.AngularSpeed / Constants.FreqEchQEI;

                    // Correction finale de l'orientation si la vitesse angulaire devient nulle (évite les erreurs d'arrondi)
                    if (GhostPosition.AngularSpeed == 0)
                    {
                        GhostPosition.Theta = targetAngle;
                    }
                }

                // Gestion du mouvement linéaire pour atteindre la cible
                if (distanceToCover != 0 && Math.Abs(angleToCover) < 0.5)
                {
                    if (distanceToCover > (stopDistance + GhostPosition.LinearSpeed / Constants.FreqEchQEI))
                    {
                        if (GhostPosition.LinearSpeed >= maxLinearSpeed) // Si la vitesse linéaire maximale calculée est atteinte
                        {
                            GhostPosition.LinearSpeed = Math.Max(GhostPosition.LinearSpeed - Constants.LinearAccel / Constants.FreqEchQEI, maxLinearSpeed);
                        }
                        else
                        {
                            // Accélération avec saturation à la vitesse maximale calculée
                            GhostPosition.LinearSpeed = Math.Min(GhostPosition.LinearSpeed + Constants.LinearAccel / Constants.FreqEchQEI, maxLinearSpeed);
                        }
                    }
                    else // Si la distance à parcourir est inférieure à la distance d'arrêt
                    {
                        // Décélération pour atteindre la cible
                        GhostPosition.LinearSpeed = Math.Max(GhostPosition.LinearSpeed - Constants.LinearAccel / Constants.FreqEchQEI, 0);
                    }

                    // Si la distance à parcourir devient très faible (fin du déplacement)
                    if (distanceToCover <= maxStopRadius)
                    {
                        // Correction finale de la position du ghost pour éviter les erreurs d'arrondi
                        GhostPosition.X = GhostPosition.TargetX;
                        GhostPosition.Y = GhostPosition.TargetY;
                        GhostPosition.LinearSpeed = 0;
                    }
                }

                // Mise à jour de la position du ghost en fonction de la vitesse linéaire
                double deltaCovered = GhostPosition.LinearSpeed / Constants.FreqEchQEI;
                GhostPosition.X += deltaCovered * Math.Cos(GhostPosition.Theta);
                GhostPosition.Y += deltaCovered * Math.Sin(GhostPosition.Theta);
            }
        }
    }
}