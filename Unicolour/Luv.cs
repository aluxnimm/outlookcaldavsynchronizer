using System;

namespace Wacton.Unicolour
{

    using static Utils;

    public record Luv : ColourRepresentation
    {
        protected override int? HueIndex => null;
        public double L => First;
        public double U => Second;
        public double V => Third;
        internal override bool IsGreyscale => L is <= 0.0 or >= 100.0 || (U.Equals(0.0) && V.Equals(0.0));

        public Luv(double l, double u, double v) : this(l, u, v, ColourHeritage.None)
        {
        }

        internal Luv(double l, double u, double v, ColourHeritage heritage) : base(l, u, v, heritage)
        {
        }

        protected override string FirstString => $"{L:F2}";
        protected override string SecondString => $"{U:+0.00;-0.00;0.00}";
        protected override string ThirdString => $"{V:+0.00;-0.00;0.00}";
        public override string ToString() => base.ToString();

        /*
         * LUV is a transform of XYZ
         * Forward: https://en.wikipedia.org/wiki/CIELUV#The_forward_transformation
         * Reverse: https://en.wikipedia.org/wiki/CIELUV#The_reverse_transformation
         */

        internal static Luv FromXyz(Xyz xyz, XyzConfiguration xyzConfig)
        {
            var (x, y, z) = xyz.Triplet;
            var (xRef, yRef, zRef) = xyzConfig.WhitePoint;

            double U(double xu, double yu, double zu) => 4 * xu / (xu + 15 * yu + 3 * zu);
            double V(double xv, double yv, double zv) => 9 * yv / (xv + 15 * yv + 3 * zv);
            var uPrime = U(x * 100, y * 100, z * 100);
            var uPrimeRef = U(xRef, yRef, zRef);
            var vPrime = V(x * 100, y * 100, z * 100);
            var vPrimeRef = V(xRef, yRef, zRef);

            var yRatio = y * 100 / yRef;
            var l = yRatio > Math.Pow(6.0 / 29.0, 3) ? 116 * CubeRoot(yRatio) - 16 : Math.Pow(29 / 3.0, 3) * yRatio;
            var u = 13 * l * (uPrime - uPrimeRef);
            var v = 13 * l * (vPrime - vPrimeRef);

            double ZeroNaN(double value) => double.IsNaN(value) ? 0.0 : value;
            return new Luv(ZeroNaN(l), ZeroNaN(u), ZeroNaN(v), ColourHeritage.From(xyz));
        }

        internal static Xyz ToXyz(Luv luv, XyzConfiguration xyzConfig)
        {
            var (l, u, v) = luv.Triplet;
            double U(double x, double y, double z) => 4 * x / (x + 15 * y + 3 * z);
            double V(double x, double y, double z) => 9 * y / (x + 15 * y + 3 * z);

            var (xRef, yRef, zRef) = xyzConfig.WhitePoint;
            var uPrimeRef = U(xRef, yRef, zRef);
            var uPrime = u / (13 * l) + uPrimeRef;
            var vPrimeRef = V(xRef, yRef, zRef);
            var vPrime = v / (13 * l) + vPrimeRef;

            var y = (l > 8 ? yRef * Math.Pow((l + 16) / 116.0, 3) : yRef * l * Math.Pow(3 / 29.0, 3)) / 100.0;
            var x = y * ((9 * uPrime) / (4 * vPrime));
            var z = y * ((12 - 3 * uPrime - 20 * vPrime) / (4 * vPrime));

            double ZeroNaN(double value) => double.IsNaN(value) || double.IsInfinity(value) ? 0.0 : value;
            return new Xyz(ZeroNaN(x), ZeroNaN(y), ZeroNaN(z), ColourHeritage.From(luv));
        }
    }
}