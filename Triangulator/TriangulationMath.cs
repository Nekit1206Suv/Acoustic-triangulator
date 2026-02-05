using System;
using System.Windows;

namespace Triangulator
{
    public struct TriangulationResult
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Height { get; set; }
        public bool IsValid { get; set; }
        public string Error { get; set; }
    }

    public static class TriangulationMath
    {
        public static double ToRad(double deg) => deg * Math.PI / 180.0;

        private static Point? IntersectLines(double x1, double y1, double angle1, double x2, double y2, double angle2)
        {
            double a1 = -Math.Sin(angle1);
            double b1 = Math.Cos(angle1);
            double c1 = a1 * x1 + b1 * y1;

            double a2 = -Math.Sin(angle2);
            double b2 = Math.Cos(angle2);
            double c2 = a2 * x2 + b2 * y2;

            double det = a1 * b2 - a2 * b1;
            if (Math.Abs(det) < 1e-9) return null;

            double x = (b2 * c1 - b1 * c2) / det;
            double y = (a1 * c2 - a2 * c1) / det;

            return new Point(x, y);
        }

        public static TriangulationResult CalculatePosition(double L, double el1, double az1, double el2, double az2, double el3, double az3)
        {
            if (L <= 0) return new TriangulationResult { IsValid = false, Error = "Invalid L" };

            var S1 = new Point(0, 0);
            var S2 = new Point(L, 0);
            var S3 = new Point(L / 2, L * Math.Sqrt(3) / 2);
            var C = new Point(L / 2, L * Math.Sqrt(3) / 6);

            double baseAngle1 = Math.Atan2(C.Y - S1.Y, C.X - S1.X);
            double baseAngle2 = Math.Atan2(C.Y - S2.Y, C.X - S2.X);
            double baseAngle3 = Math.Atan2(C.Y - S3.Y, C.X - S3.X);

            double theta1 = baseAngle1 - ToRad(az1);
            double theta2 = baseAngle2 - ToRad(az2);
            double theta3 = baseAngle3 - ToRad(az3);

            var p12 = IntersectLines(S1.X, S1.Y, theta1, S2.X, S2.Y, theta2);
            var p23 = IntersectLines(S2.X, S2.Y, theta2, S3.X, S3.Y, theta3);
            var p31 = IntersectLines(S3.X, S3.Y, theta3, S1.X, S1.Y, theta1);

            if (p12 == null || p23 == null || p31 == null)
                return new TriangulationResult { IsValid = false, Error = "No Intersection" };

            double x = (p12.Value.X + p23.Value.X + p31.Value.X) / 3.0;
            double y = (p12.Value.Y + p23.Value.Y + p31.Value.Y) / 3.0;

            double dist1 = Math.Sqrt(Math.Pow(x - S1.X, 2) + Math.Pow(y - S1.Y, 2));
            double dist2 = Math.Sqrt(Math.Pow(x - S2.X, 2) + Math.Pow(y - S2.Y, 2));
            double dist3 = Math.Sqrt(Math.Pow(x - S3.X, 2) + Math.Pow(y - S3.Y, 2));

            double SafeTan(double deg) => Math.Tan(ToRad(Math.Clamp(deg, 0, 89.9)));

            double h1 = dist1 * SafeTan(el1);
            double h2 = dist2 * SafeTan(el2);
            double h3 = dist3 * SafeTan(el3);

            return new TriangulationResult
            {
                X = x,
                Y = y,
                Height = (h1 + h2 + h3) / 3.0,
                IsValid = true
            };
        }
    }
}