using System;
using System.Collections.Generic;

namespace Wacton.Unicolour
{

    using static Utils;

    public static class Cam
    {
        public record Model(double J, double C, double H, double M, double S, double Q)
        {
            public double J { get; } = J;
            public double C { get; } = C;
            public double H { get; } = H;
            public string Hc { get; } = HueData.GetHueComposition(H); // technically not in CAM02 but 🤷
            public double M { get; } = M;
            public double S { get; } = S;
            public double Q { get; } = Q;

            public double Lightness => J;
            public double Chroma => C;
            public double HueAngle => H;
            public string HueComposition => Hc;
            public double Colourfulness => M;
            public double Saturation => S;
            public double Brightness => Q;

            internal Ucs ToUcs()
            {
                var j = 1.7 * J / (1 + 0.007 * J);
                var m = Math.Log(1 + 0.0228 * M) / 0.0228;
                (j, var a, var b) = FromLchTriplet(new(j, m, H));
                return new Ucs(j, a, b);
            }
        }

        /*
         * NOTE: if ever want to support CAM02-LCD & CAM02-SCD (Large / Small Colour Difference)
         * see Table 2.2 in "CIECAM02 and Its Recent Developments" (https://doi.org/10.1007/978-1-4419-6190-7_2)
         * essentially a small tweak to how j / m / ΔE' are calculated
         * but would need to consider how it affects the structure of Unicolour objects
         */
        public record Ucs(double J, double A, double B)
        {
            public double J { get; } = J;
            public double A { get; } = A;
            public double B { get; } = B;

            internal Model ToModel(ViewingConditions view)
            {
                var j = J / (1.7 - 0.007 * J);
                (j, var m, var h) = ToLchTriplet(j, A, B);
                m = (Math.Exp(0.0228 * m) - 1) / 0.0228;

                var q = 4 / view.C * Math.Pow(j / 100.0, 0.5) * (view.Aw + 4) * Math.Pow(view.Fl, 0.25);
                var c = m / Math.Pow(view.Fl, 0.25);
                var s = 100 * Math.Pow(m / q, 0.5);
                return new Model(j, c, h, m, s, q);
            }
        }

        internal record ViewingConditions(
            double C, double Nc, double Dr, double Dg, double Db,
            double Fl, double N, double Z, double Nbb, double Ncb, double Aw)
        {
            public double C { get; } = C;
            public double Nc { get; } = Nc;
            public double Dr { get; } = Dr;
            public double Dg { get; } = Dg;
            public double Db { get; } = Db;
            public double Fl { get; } = Fl;
            public double N { get; } = N;
            public double Z { get; } = Z;
            public double Nbb { get; } = Nbb;
            public double Ncb { get; } = Ncb;
            public double Aw { get; } = Aw;
        }

        internal static class HueData
        {
            private const double Angle1 = 20.14;
            private const double Angle2 = 90.00;
            private const double Angle3 = 164.25;
            private const double Angle4 = 237.53;
            private const double Angle5 = 380.14;

            private static readonly string[] Names = { "R", "Y", "G", "B", "R" };
            private static readonly double[] Angles = { Angle1, Angle2, Angle3, Angle4, Angle5 };
            private static readonly double[] Es = { 0.8, 0.7, 1.0, 1.2, 0.8 };
            private static readonly double[] Quads = { 0.0, 100.0, 200.0, 300.0, 400.0 };

            private static string Name(int i) => Get(Names, i);
            private static double Angle(int i) => Get(Angles, i);
            private static double E(int i) => Get(Es, i);
            private static double Quad(int i) => Get(Quads, i);
            private static T Get<T>(IReadOnlyList<T> array, int i) => array[i - 1];

            private static double GetHPrime(double h) => h < Angle1 ? h + 360 : h;
            internal static double GetEccentricity(double h) => 0.25 * (Math.Cos(ToRadians(GetHPrime(h)) + 2) + 3.8);

            internal static string GetHueComposition(double h)
            {
                if (double.IsNaN(h)) return "-";
                var hPrime = GetHPrime(h);
                int? index = hPrime switch
                {
                    >= Angle1 and < Angle2 => 1,
                    >= Angle2 and < Angle3 => 2,
                    >= Angle3 and < Angle4 => 3,
                    >= Angle4 and < Angle5 => 4,
                    _ => null
                };

                if (index == null) return "-";
                var i = index.Value;
                var hQuad = Quad(i) +
                            100 * E(i + 1) * (hPrime - Angle(i)) /
                            (E(i + 1) * (hPrime - Angle(i)) + E(i) * (Angle(i + 1) - hPrime));
                var pl = Quad(i + 1) - hQuad;
                var pr = hQuad - Quad(i);
                var hc = $"{pl:f0}{Name(i)}{pr:f0}{Name(i + 1)}";
                return hc;
            }
        }
    }
}