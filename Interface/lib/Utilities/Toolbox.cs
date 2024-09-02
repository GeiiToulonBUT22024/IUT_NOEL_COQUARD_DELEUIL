using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using System;
using System.Linq;
using System.Windows;


namespace Utilities
{
    /// <summary>
    /// Contient plusieurs fonctions mathématiques utiles.
    /// </summary>
    public static class Toolbox
    {
        /// <summary>
        /// Renvoie la valeur max d'une liste de valeurs
        /// </summary>
        public static double Max(params double[] values)
            => values.Max();

        /// <summary>Converti un angle en degrés en un angle en radians.</summary>
        public static float DegToRad(float angleDeg)
            => angleDeg * (float)Math.PI / 180f;

        /// <summary>Converti un angle en degrés en un angle en radians.</summary>
        public static double DegToRad(double angleDeg)
            => angleDeg * Math.PI / 180;

        /// <summary>Converti un angle en degrés en un angle en radians.</summary>
        public static float RadToDeg(float angleRad)
            => angleRad / (float)Math.PI * 180f;

        /// <summary>Converti un angle en radians en un angle en degrés.</summary>
        public static double RadToDeg(double angleRad)
            => angleRad / Math.PI * 180;

        /// <summary>Renvoie l'angle modulo 2*pi entre -pi et pi.</summary>
        public static double Modulo2PiAngleRad(double angleRad)
        {
            double angleTemp = (angleRad - Math.PI) % (2 * Math.PI) + Math.PI;
            return (angleTemp + Math.PI) % (2 * Math.PI) - Math.PI;
        }

        /// <summary>Renvoie l'angle modulo pi entre -pi/2 et pi/2.</summary>
        public static double ModuloPiAngleRadian(double angleRad)
        {
            double angleTemp = (angleRad - Math.PI / 2.0) % Math.PI + Math.PI / 2.0;
            return (angleTemp + Math.PI / 2.0) % Math.PI - Math.PI / 2.0;
        }


        /// <summary>Renvoie l'angle modulo pi entre -pi et pi.</summary>
        public static double ModuloPiDivTwoAngleRadian(double angleRad)
        {
            double angleTemp = (angleRad - Math.PI / 4.0) % (Math.PI / 2) + Math.PI / 4.0;
            return (angleTemp + Math.PI / 4.0) % (Math.PI / 2) - Math.PI / 4.0;
        }

        /// <summary>Borne la valeur entre les deux valeurs limites données.</summary>
        public static double LimitToInterval(double value, double lowLimit, double highLimit)
        {
            if (value > highLimit)
                return highLimit;
            else if (value < lowLimit)
                return lowLimit;
            else
                return value;
        }

        /// <summary>Décale un angle dans un intervale de [-PI, PI] autour d'un autre.</summary>
        public static double ModuloByAngle(double angleToCenterAround, double angleToCorrect)
        {
            // On corrige l'angle obtenu pour le moduloter autour de l'angle Kalman
            int decalageNbTours = (int)Math.Round((angleToCorrect - angleToCenterAround) / (2 * Math.PI));
            double thetaDest = angleToCorrect - decalageNbTours * 2 * Math.PI;

            return thetaDest;
        }

        public static double Distance(PointD pt1, PointD pt2)
        {if (pt1 != null && pt2 != null)
                return Math.Sqrt((pt2.X - pt1.X) * (pt2.X - pt1.X) + (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y));
            else 
                return double.PositiveInfinity;
            //return Math.Sqrt(Math.Pow(pt2.X - pt1.X, 2) + Math.Pow(pt2.Y - pt1.Y, 2));
        }

        public static double Distance(Location pt1, Location pt2)
        {
            if (pt1 != null && pt2 != null)
                return Math.Sqrt((pt2.X - pt1.X) * (pt2.X - pt1.X) + (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y));
            else
                return double.PositiveInfinity;
            //return Math.Sqrt(Math.Pow(pt2.X - pt1.X, 2) + Math.Pow(pt2.Y - pt1.Y, 2));
        }

        public static double Distance(PolarPointRssi pt1, PolarPointRssi pt2)
        {
            return Math.Sqrt(pt1.Distance * pt1.Distance + pt2.Distance * pt2.Distance - 2 * pt1.Distance * pt2.Distance * Math.Cos(pt1.Angle - pt2.Angle));
        }

        public static double DistanceL1(PointD pt1, PointD pt2)
        {
            return Math.Abs(pt2.X - pt1.X) + Math.Abs(pt2.Y - pt1.Y);
            //return Math.Sqrt(Math.Pow(pt2.X - pt1.X, 2) + Math.Pow(pt2.Y - pt1.Y, 2));
        }

        public static double Distance(double xPt1, double yPt1, double xPt2, double yPt2)
        {
            return Math.Sqrt(Math.Pow(xPt2 - xPt1, 2) + Math.Pow(yPt2 - yPt1, 2));
        }

        public static double DistancePointToLine(PointD pt, PointD LinePt, double LineAngle)
        {
            var xLineVect = Math.Cos(LineAngle);
            var yLineVect = Math.Sin(LineAngle);
            var dot = (pt.X - LinePt.X) * (yLineVect) - (pt.Y - LinePt.Y) * (xLineVect);
            return Math.Abs(dot);
        }
        
        public static double DistancePointToSegment(PointD pt, PointD ptSeg1, PointD ptSeg2)
        {
            var A = pt.X - ptSeg1.X;
            var B = pt.Y - ptSeg1.Y;
            var C = ptSeg2.X - ptSeg1.X;
            var D = ptSeg2.Y - ptSeg1.Y;

            double dot = A * C + B * D;
            double len_sq = C * C + D * D;
            double param = -1;
            if (len_sq != 0) //in case of 0 length line
                param = dot / len_sq;

            double xx, yy;

            if (param < 0)
            {
                xx = ptSeg1.X;
                yy = ptSeg1.Y;
            }
            else if (param > 1)
            {
                xx = ptSeg2.X;
                yy = ptSeg2.Y;
            }
            else
            {
                xx = ptSeg1.X + param * C;
                yy = ptSeg1.Y + param * D;
            }

            var dx = pt.X - xx;
            var dy = pt.Y - yy;

            double distance = Math.Sqrt(dx * dx + dy * dy);
            return distance;            
        }

        public static double DistancePointToSegment(PointD pt, SegmentD segment)
        {
            var ptSeg1 = segment.PtDebut;
            var ptSeg2 = segment.PtFin;

            return DistancePointToSegment(pt, ptSeg1, ptSeg2);
        }

        public static PointD GetInterceptionLocation(LocationExtended target, Location hunter, double huntingSpeed)
        {
            //D'après Al-Kashi, si d est la distance entre le pt target et le pt chasseur, que les vitesses sont constantes 
            //et égales à Vtarget et Vhunter
            //Rappel Al Kashi : A² = B²+C²-2BCcos(alpha) , alpha angle opposé au segment A
            //On a au moment de l'interception à l'instant Tinter: 
            //A = Vh * Tinter
            //B = VT * Tinter
            //C = initialDistance;
            //alpha = Pi - capCible - angleCible

            double targetSpeed = Math.Sqrt(Math.Pow(target.Vx, 2) + Math.Pow(target.Vy, 2));
            double initialDistance = Toolbox.Distance(new PointD(hunter.X, hunter.Y), new PointD(target.X, target.Y));
            double capCible = Math.Atan2(target.Vy, target.Vx);
            double angleCible = Math.Atan2(target.Y - hunter.Y, target.X - hunter.X);
            double angleCapCibleDirectionCibleChasseur = Math.PI - capCible + angleCible;

            //Résolution de ax²+bx+c=0 pour trouver Tinter
            double a = Math.Pow(huntingSpeed, 2) - Math.Pow(targetSpeed, 2);
            double b = 2 * initialDistance * targetSpeed * Math.Cos(angleCapCibleDirectionCibleChasseur);
            double c = -Math.Pow(initialDistance, 2);

            double delta = b * b - 4 * a * c;
            double t1 = (-b - Math.Sqrt(delta)) / (2 * a);
            double t2 = (-b + Math.Sqrt(delta)) / (2 * a);

            if (delta > 0 && t2 < 10)
            {
                double xInterception = target.X + targetSpeed * Math.Cos(capCible) * t2;
                double yInterception = target.Y + targetSpeed * Math.Sin(capCible) * t2;
                return new PointD(xInterception, yInterception);
            }
            else
                return null;
        }

        public static PointD OffsetLocation(PointD P, Location PtRef, bool invertOffset = false)
        {
            if (invertOffset == false)
            {
                var VectRef0x = P.X - PtRef.X;
                var VectRef0y = P.Y - PtRef.Y;
                var VectRefPtx = Math.Cos(-PtRef.Theta) * VectRef0x - Math.Sin(-PtRef.Theta) * VectRef0y;
                var VectRefPty = Math.Sin(-PtRef.Theta) * VectRef0x + Math.Cos(-PtRef.Theta) * VectRef0y;
                return new PointD(VectRefPtx, VectRefPty);
            }
            else
            {
                var VectRefPtx = P.X;
                var VectRefPty = P.Y;
                var VectRef0x = Math.Cos(+PtRef.Theta) * VectRefPtx - Math.Sin(+PtRef.Theta) * VectRefPty;
                var VectRef0y = Math.Sin(+PtRef.Theta) * VectRefPtx + Math.Cos(+PtRef.Theta) * VectRefPty;
                return new PointD(VectRef0x + PtRef.X, VectRef0y + PtRef.Y);
            }
        }

        public static Location OffsetLocation(Location l, Location offset)
        {
            var angle = l.Theta - offset.Theta;
            var vangle = l.Vtheta - offset.Vtheta;
            var xTranslate = l.X - offset.X;
            var yTranslate = l.Y - offset.Y;
            var xOffset = Math.Cos(-offset.Theta) * xTranslate - Math.Sin(-offset.Theta) * yTranslate;
            var yOffset = Math.Sin(-offset.Theta) * xTranslate + Math.Cos(-offset.Theta) * yTranslate;
            var vxTranslate = l.Vx - offset.Vx;
            var vyTranslate = l.Vy - offset.Vy;
            var vxOffset = Math.Cos(-offset.Theta) * vxTranslate - Math.Sin(-offset.Theta) * vyTranslate;
            var vyOffset = Math.Sin(-offset.Theta) * vxTranslate + Math.Cos(-offset.Theta) * vyTranslate;
            return new Location(xOffset, yOffset, angle, vxOffset, vyOffset, 0);
        }

        public static LocationExtended OffsetLocation(LocationExtended l, Location offset)
        {
            var angle = l.Theta - offset.Theta;
            var vangle = l.Vtheta - offset.Vtheta;
            var xTranslate = l.X - offset.X;
            var yTranslate = l.Y - offset.Y;
            var xOffset = Math.Cos(-offset.Theta) * xTranslate - Math.Sin(-offset.Theta) * yTranslate;
            var yOffset = Math.Sin(-offset.Theta) * xTranslate + Math.Cos(-offset.Theta) * yTranslate;
            var vxTranslate = l.Vx - offset.Vx;
            var vyTranslate = l.Vy - offset.Vy;
            var vxOffset = Math.Cos(offset.Theta) * vxTranslate - Math.Sin(offset.Theta) * vyTranslate;
            var vyOffset = Math.Sin(offset.Theta) * vxTranslate + Math.Cos(offset.Theta) * vyTranslate;
            return new LocationExtended(xOffset, yOffset, angle, vxOffset, vyOffset, 0, l.Type);
        }               

        public static double ScalarProduct(SegmentExtended s1, SegmentExtended s2)
        {
            return (s1.Segment.X2 - s1.Segment.X1) * (s2.Segment.X2 - s2.Segment.X1) + (s1.Segment.Y2 - s1.Segment.Y1) * (s2.Segment.Y2 - s2.Segment.Y1);
        }
        public static double ScalarProduct(SegmentD s1, SegmentD s2)
        {
            return (s1.X2 - s1.X1) * (s2.X2 - s2.X1) + (s1.Y2 - s1.Y1) * (s2.Y2 - s2.Y1);
        }
        public static double Angle(SegmentD s1, SegmentD s2)
        {
            return Toolbox.Modulo2PiAngleRad(s2.Angle- s1.Angle);
        }
        public static double Angle(SegmentExtended s1, SegmentExtended s2)
        {
            return Toolbox.Modulo2PiAngleRad(s2.Segment.Angle - s1.Segment.Angle);
        }
        public static double VectorProduct(SegmentD s1, SegmentD s2)
        {
            return (s1.X2 - s1.X1) * (s2.Y2 - s2.Y1) - (s1.Y2 - s1.Y1) * (s2.X2 - s2.X1);
        }

        public static double? Cross(Vector<double> left, Vector<double> right)
        {
            double? result = null;
            if ((left.Count == 2 && right.Count == 2))
            {
                result = left[0] * right[1] - left[1] * right[0];
            }
            return result;
        }
    }
}

