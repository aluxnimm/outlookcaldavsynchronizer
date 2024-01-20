using System;

namespace Wacton.Unicolour
{

    public record Xyy : ColourRepresentation
    {
        protected override int? HueIndex => null;
        public Chromaticity Chromaticity => new(First, Second);
        public double Luminance => Third;
        public Chromaticity ConstrainedChromaticity => new(ConstrainedFirst, ConstrainedSecond);
        public double ConstrainedLuminance => ConstrainedThird;
        protected override double ConstrainedFirst => Math.Max(Chromaticity.X, 0);
        protected override double ConstrainedSecond => Math.Max(Chromaticity.Y, 0);
        protected override double ConstrainedThird => Math.Max(Luminance, 0);

        // could compare chromaticity against config.ChromaticityWhite
        // but requires making assumptions about floating-point comparison, which I don't want to do
        internal override bool IsGreyscale => Luminance <= 0.0;

        public Xyy(double x, double y, double upperY) : this(x, y, upperY, ColourHeritage.None)
        {
        }

        internal Xyy(double x, double y, double upperY, ColourHeritage heritage) : base(x, y, upperY, heritage)
        {
        }

        protected override string FirstString => $"{Chromaticity.X:F4}";
        protected override string SecondString => $"{Chromaticity.Y:F4}";
        protected override string ThirdString => $"{Luminance:F4}";
        public override string ToString() => base.ToString();

        /*
         * XYY is a transform of XYZ (in terms of Unicolour implementation)
         * Forward: https://en.wikipedia.org/wiki/CIE_1931_color_space#CIE_xy_chromaticity_diagram_and_the_CIE_xyY_color_space
         * Reverse: https://en.wikipedia.org/wiki/CIE_1931_color_space#CIE_xy_chromaticity_diagram_and_the_CIE_xyY_color_space
         */

        internal static Xyy FromXyz(Xyz xyz, XyzConfiguration xyzConfig)
        {
            var (x, y, z) = xyz.Triplet;
            var normalisation = x + y + z;
            var isBlack = normalisation == 0.0;

            var chromaticityX = isBlack ? xyzConfig.ChromaticityWhite.X : x / normalisation;
            var chromaticityY = isBlack ? xyzConfig.ChromaticityWhite.Y : y / normalisation;
            var luminance = isBlack ? 0 : y;
            return new Xyy(chromaticityX, chromaticityY, luminance, ColourHeritage.From(xyz));
        }

        internal static Xyz ToXyz(Xyy xyy)
        {
            var chromaticity = xyy.ConstrainedChromaticity;
            var luminance = xyy.ConstrainedLuminance;

            var useZero = chromaticity.Y <= 0;
            var factor = luminance / chromaticity.Y;
            var x = useZero ? 0 : factor * chromaticity.X;
            var y = useZero ? 0 : luminance;
            var z = useZero ? 0 : factor * (1 - chromaticity.X - chromaticity.Y);
            return new Xyz(x, y, z, ColourHeritage.From(xyy));
        }
    }
}