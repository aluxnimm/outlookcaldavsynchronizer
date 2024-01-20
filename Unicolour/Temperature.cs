using System;

namespace Wacton.Unicolour
{

    public record Temperature(double Cct, double Duv)
    {
        public double Cct { get; } = Cct;
        public double Duv { get; } = Duv;

        private static Temperature NaN() => new(double.NaN, double.NaN);

        // https://en.wikipedia.org/wiki/CIE_1960_color_space
        internal static Temperature Get(Xyz xyz)
        {
            var (x, y, z) = xyz.Triplet;
            var u = 4.0 * x / (x + 15.0 * y + 3.0 * z);
            var v = 6.0 * y / (x + 15.0 * y + 3.0 * z);

            return double.IsNaN(u) || double.IsNaN(v) ? NaN() : Get(u, v);
        }

        // https://en.wikipedia.org/wiki/Correlated_color_temperature#Robertson's_method
        internal static Temperature Get(double u, double v)
        {
            for (var i = 0; i < Isotherms.Length - 1; i++)
            {
                var lowerIsotherm = Isotherms[i];
                var upperIsotherm = Isotherms[i + 1];

                // not actually the distance yet, but can be used to inform if between isotherms
                var lowerDistance = v - lowerIsotherm.V - lowerIsotherm.M * (u - lowerIsotherm.U);
                var upperDistance = v - upperIsotherm.V - upperIsotherm.M * (u - upperIsotherm.U);

                // when distance to the 2 isotherms are in different directions, test point (u, v) is between them
                var isBetweenIsotherms = Math.Sign(lowerDistance) != Math.Sign(upperDistance);
                if (!isBetweenIsotherms)
                {
                    continue;
                }

                // once between isotherms, can calculate the distance and interpolate
                lowerDistance /= Math.Sqrt(1.0 + Math.Pow(lowerIsotherm.M, 2));
                upperDistance /= Math.Sqrt(1.0 + Math.Pow(upperIsotherm.M, 2));

                // signs are different, will add the distances together
                var totalDistance = lowerDistance - upperDistance;
                var interpolationDistance = lowerDistance / totalDistance;
                var reciprocalMegakelvin = Interpolation.Interpolate(lowerIsotherm.ReciprocalMegakelvin,
                    upperIsotherm.ReciprocalMegakelvin, interpolationDistance);
                var kelvins = 1000000 / reciprocalMegakelvin;
                var duv = GetDuv(u, v);
                return new Temperature(kelvins, duv);
            }

            return NaN();
        }

        // https://doi.org/10.1080/15502724.2014.839020
        private static double GetDuv(double u, double v)
        {
            const double k6 = -0.00616793;
            const double k5 = 0.0893944;
            const double k4 = -0.5179722;
            const double k3 = 1.5317403;
            const double k2 = -2.4243787;
            const double k1 = 1.925865;
            const double k0 = -0.471106;

            var lfp = Math.Sqrt(Math.Pow(u - 0.292, 2) + Math.Pow(v - 0.240, 2));
            var a = Math.Acos((u - 0.292) / lfp);
            var lbb = k6 * Math.Pow(a, 6) + k5 * Math.Pow(a, 5) + k4 * Math.Pow(a, 4) +
                      k3 * Math.Pow(a, 3) + k2 * Math.Pow(a, 2) + k1 * a + k0;
            return lfp - lbb;
        }

        private record Isotherm(double U, double V, double M, double ReciprocalMegakelvin)
        {
            public double U { get; } = U;
            public double V { get; } = V;
            public double M { get; } = M;
            public double ReciprocalMegakelvin { get; } = ReciprocalMegakelvin; // also known as mired
        }

        // http://www.brucelindbloom.com/Eqn_XYZ_to_T.html
        private static readonly Isotherm[] Isotherms =
        {
            new(0.18006, 0.26352, -0.24341, double.Epsilon),
            new(0.18066, 0.26589, -0.25479, 10),
            new(0.18133, 0.26846, -0.26876, 20),
            new(0.18208, 0.27119, -0.28539, 30),
            new(0.18293, 0.27407, -0.30470, 40),
            new(0.18388, 0.27709, -0.32675, 50),
            new(0.18494, 0.28021, -0.35156, 60),
            new(0.18611, 0.28342, -0.37915, 70),
            new(0.18740, 0.28668, -0.40955, 80),
            new(0.18880, 0.28997, -0.44278, 90),
            new(0.19032, 0.29326, -0.47888, 100),
            new(0.19462, 0.30141, -0.58204, 125),
            new(0.19962, 0.30921, -0.70471, 150),
            new(0.20525, 0.31647, -0.84901, 175),
            new(0.21142, 0.32312, -1.0182, 200),
            new(0.21807, 0.32909, -1.2168, 225),
            new(0.22511, 0.33439, -1.4512, 250),
            new(0.23247, 0.33904, -1.7298, 275),
            new(0.24010, 0.34308, -2.0637, 300),
            new(0.24792, 0.34655, -2.4681, 325),
            new(0.25591, 0.34951, -2.9641, 350),
            new(0.26400, 0.35200, -3.5814, 375),
            new(0.27218, 0.35407, -4.3633, 400),
            new(0.28039, 0.35577, -5.3762, 425),
            new(0.28863, 0.35714, -6.7262, 450),
            new(0.29685, 0.35823, -8.5955, 475),
            new(0.30505, 0.35907, -11.324, 500),
            new(0.31320, 0.35968, -15.628, 525),
            new(0.32129, 0.36011, -23.325, 550),
            new(0.32931, 0.36038, -40.770, 575),
            new(0.33724, 0.36051, -116.45, 600)
        };

        public override string ToString() =>
            double.IsNaN(Cct) || double.IsNaN(Duv) ? "-" : $"{Cct:F1} K (Δuv {Duv:F5})";
    }
}