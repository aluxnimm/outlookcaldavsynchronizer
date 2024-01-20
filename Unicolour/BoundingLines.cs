using System;
using System.Collections.Generic;
using System.Linq;

namespace Wacton.Unicolour
{

    internal static class BoundingLines
    {
        internal static double CalculateMaxChroma(double lightness, double hue)
        {
            var hueRad = hue / 360 * Math.PI * 2;
            return GetBoundingLines(lightness).Select(x => DistanceFromOriginAngle(hueRad, x)).Min();
        }

        internal static double CalculateMaxChroma(double lightness)
        {
            return GetBoundingLines(lightness).Select(DistanceFromOrigin).Min();
        }

        // https://github.com/hsluv/hsluv-haxe/blob/master/src/hsluv/Hsluv.hx#L249
        private static IEnumerable<Line> GetBoundingLines(double l)
        {
            const double kappa = 903.2962962;
            const double epsilon = 0.0088564516;
            var matrixR = Matrix.FromTriplet(3.240969941904521, -1.537383177570093, -0.498610760293);
            var matrixG = Matrix.FromTriplet(-0.96924363628087, 1.87596750150772, 0.041555057407175);
            var matrixB = Matrix.FromTriplet(0.055630079696993, -0.20397695888897, 1.056971514242878);

            var sub1 = Math.Pow(l + 16, 3) / 1560896;
            var sub2 = sub1 > epsilon ? sub1 : l / kappa;

            IEnumerable<Line> CalculateLines(Matrix matrix)
            {
                var s1 = sub2 * (284517 * matrix[0, 0] - 94839 * matrix[2, 0]);
                var s2 = sub2 * (838422 * matrix[2, 0] + 769860 * matrix[1, 0] + 731718 * matrix[0, 0]);
                var s3 = sub2 * (632260 * matrix[2, 0] - 126452 * matrix[1, 0]);

                var slope0 = s1 / s3;
                var intercept0 = s2 * l / s3;
                var slope1 = s1 / (s3 + 126452);
                var intercept1 = (s2 - 769860) * l / (s3 + 126452);
                return new[] { new Line(slope0, intercept0), new Line(slope1, intercept1) };
            }

            var lines = new List<Line>();
            lines.AddRange(CalculateLines(matrixR));
            lines.AddRange(CalculateLines(matrixG));
            lines.AddRange(CalculateLines(matrixB));
            return lines;
        }

        private static double DistanceFromOriginAngle(double theta, Line line)
        {
            var distance = line.Intercept / (Math.Sin(theta) - line.Slope * Math.Cos(theta));
            return distance < 0 ? double.PositiveInfinity : distance;
        }

        private static double DistanceFromOrigin(Line line) =>
            Math.Abs(line.Intercept) / Math.Sqrt(Math.Pow(line.Slope, 2) + 1);

        private record Line(double Slope, double Intercept)
        {
            public double Slope { get; } = Slope;
            public double Intercept { get; } = Intercept;
        }
    }
}