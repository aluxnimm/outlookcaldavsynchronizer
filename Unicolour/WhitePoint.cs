using System.Collections.Generic;

namespace Wacton.Unicolour
{

    public record WhitePoint(double X, double Y, double Z)
    {
        public double X { get; } = X;
        public double Y { get; } = Y;
        public double Z { get; } = Z;
        internal Matrix AsXyzMatrix() => Matrix.FromTriplet(X, Y, Z).Select(x => x / 100.0);
        public override string ToString() => $"({X}, {Y}, {Z})";

        public static WhitePoint From(Illuminant illuminant, Observer observer = Observer.Standard2)
        {
            return ByIlluminant[observer][illuminant];
        }

        public static WhitePoint From(Chromaticity chromaticity)
        {
            var xyz = Xyy.ToXyz(new(chromaticity.X, chromaticity.Y, 1));
            return new WhitePoint(xyz.X * 100, xyz.Y * 100, xyz.Z * 100);
        }

        // as far as I'm aware, these are the latest ASTM standards
        private static readonly Dictionary<Observer, Dictionary<Illuminant, WhitePoint>> ByIlluminant = new()
        {
            {
                Observer.Standard2, new()
                {
                    { Illuminant.A, new(109.850, 100.000, 35.585) },
                    { Illuminant.C, new(98.074, 100.000, 118.232) },
                    { Illuminant.D50, new(96.422, 100.000, 82.521) },
                    { Illuminant.D55, new(95.682, 100.000, 92.149) },
                    { Illuminant.D65, new(95.047, 100.000, 108.883) },
                    { Illuminant.D75, new(94.972, 100.000, 122.638) },
                    { Illuminant.E, new(100.000, 100.000, 100.000) },
                    { Illuminant.F2, new(99.186, 100.000, 67.393) },
                    { Illuminant.F7, new(95.041, 100.000, 108.747) },
                    { Illuminant.F11, new(100.962, 100.000, 64.350) }
                }
            },
            {
                Observer.Supplementary10, new()
                {
                    { Illuminant.A, new(111.144, 100.000, 35.200) },
                    { Illuminant.C, new(97.285, 100.000, 116.145) },
                    { Illuminant.D50, new(96.720, 100.000, 81.427) },
                    { Illuminant.D55, new(95.799, 100.000, 90.926) },
                    { Illuminant.D65, new(94.811, 100.000, 107.304) },
                    { Illuminant.D75, new(94.416, 100.000, 120.641) },
                    { Illuminant.E, new(100.000, 100.000, 100.000) },
                    { Illuminant.F2, new(103.279, 100.000, 69.027) },
                    { Illuminant.F7, new(95.792, 100.000, 107.686) },
                    { Illuminant.F11, new(103.863, 100.000, 65.607) }
                }
            }
        };
    }

    public enum Illuminant
    {
        A,
        C,
        D50,
        D55,
        D65,
        D75,
        E,
        F2,
        F7,
        F11
    }

    public enum Observer
    {
        Standard2,
        Supplementary10
    }
}