using System;

namespace Wacton.Unicolour
{

    using static Utils;

    public record Lab : ColourRepresentation
    {
        protected override int? HueIndex => null;
        public double L => First;
        public double A => Second;
        public double B => Third;
        internal override bool IsGreyscale => L is <= 0.0 or >= 100.0 || (A.Equals(0.0) && B.Equals(0.0));

        public Lab(double l, double a, double b) : this(l, a, b, ColourHeritage.None)
        {
        }

        internal Lab(double l, double a, double b, ColourHeritage heritage) : base(l, a, b, heritage)
        {
        }

        protected override string FirstString => $"{L:F2}";
        protected override string SecondString => $"{A:+0.00;-0.00;0.00}";
        protected override string ThirdString => $"{B:+0.00;-0.00;0.00}";
        public override string ToString() => base.ToString();

        /*
         * LAB is a transform of XYZ
         * Forward: https://en.wikipedia.org/wiki/CIELAB_color_space#From_CIEXYZ_to_CIELAB
         * Reverse: https://en.wikipedia.org/wiki/CIELAB_color_space#From_CIELAB_to_CIEXYZ
         */

        // ReSharper disable InconsistentNaming
        private const double delta = 6.0 / 29.0;
        // ReSharper restore InconsistentNaming

        internal static Lab FromXyz(Xyz xyz, XyzConfiguration xyzConfig)
        {
            var (x, y, z) = xyz.Triplet;
            var referenceWhite = xyzConfig.WhitePoint;
            var xRatio = x * 100 / referenceWhite.X;
            var yRatio = y * 100 / referenceWhite.Y;
            var zRatio = z * 100 / referenceWhite.Z;

            double F(double t) =>
                t > Math.Pow(delta, 3) ? CubeRoot(t) : t * (1 / 3.0) * Math.Pow(delta, -2) + 4.0 / 29.0;

            var l = 116 * F(yRatio) - 16;
            var a = 500 * (F(xRatio) - F(yRatio));
            var b = 200 * (F(yRatio) - F(zRatio));
            return new Lab(l, a, b, ColourHeritage.From(xyz));
        }

        internal static Xyz ToXyz(Lab lab, XyzConfiguration xyzConfig)
        {
            var (l, a, b) = lab.Triplet;
            var referenceWhite = xyzConfig.WhitePoint;
            double F(double t) => t > delta ? Math.Pow(t, 3.0) : 3 * Math.Pow(delta, 2) * (t - 4.0 / 29.0);
            var x = referenceWhite.X / 100.0 * F((l + 16) / 116.0 + a / 500.0);
            var y = referenceWhite.Y / 100.0 * F((l + 16) / 116.0);
            var z = referenceWhite.Z / 100.0 * F((l + 16) / 116.0 - b / 200.0);
            return new Xyz(x, y, z, ColourHeritage.From(lab));
        }
    }
}