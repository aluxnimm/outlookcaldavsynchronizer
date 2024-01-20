using System;

namespace Wacton.Unicolour
{

    public record Rgb : ColourRepresentation
    {
        protected override int? HueIndex => null;
        public double R => First;
        public double G => Second;
        public double B => Third;
        public double ConstrainedR => ConstrainedFirst;
        public double ConstrainedG => ConstrainedSecond;
        public double ConstrainedB => ConstrainedThird;
        protected override double ConstrainedFirst => R.Clamp(0.0, 1.0);
        protected override double ConstrainedSecond => G.Clamp(0.0, 1.0);
        protected override double ConstrainedThird => B.Clamp(0.0, 1.0);
        internal override bool IsGreyscale => ConstrainedR.Equals(ConstrainedG) && ConstrainedG.Equals(ConstrainedB);

        // for almost all cases, doing this check in linear RGB will return the same result
        // but handling it here feels most natural as it is the intended "display" space
        // and isn't concerned about questionable custom inverse-companding-to-linear functions (e.g. where where RGB <= 1.0 but RGB-Linear > 1.0)
        internal bool IsInGamut => !UseAsNaN && R is >= 0 and <= 1.0 && G is >= 0 and <= 1.0 && B is >= 0 and <= 1.0;
        public Rgb255 Byte255 => new(To255(R), To255(G), To255(B), ColourHeritage.From(this));

        public Rgb(double r, double g, double b) : this(r, g, b, ColourHeritage.None)
        {
        }

        internal Rgb(ColourTriplet triplet, ColourHeritage heritage) : this(triplet.First, triplet.Second,
            triplet.Third, heritage)
        {
        }

        internal Rgb(double r, double g, double b, ColourHeritage heritage) : base(r, g, b, heritage)
        {
        }

        private static double To255(double value) => Math.Round(value * 255);

        protected override string FirstString => $"{R:F2}";
        protected override string SecondString => $"{G:F2}";
        protected override string ThirdString => $"{B:F2}";
        public override string ToString() => base.ToString();

        /*
         * RGB is a transform of RGB Linear
         * Forward: https://en.wikipedia.org/wiki/SRGB#From_CIE_XYZ_to_sRGB
         * Reverse: https://en.wikipedia.org/wiki/SRGB#From_sRGB_to_CIE_XYZ
         */

        internal static Rgb FromRgbLinear(RgbLinear rgbLinear, RgbConfiguration rgbConfig)
        {
            var rgbLinearMatrix = Matrix.FromTriplet(rgbLinear.Triplet);
            var rgbMatrix = rgbLinearMatrix.Select(rgbConfig.CompandFromLinear);
            return new Rgb(rgbMatrix.ToTriplet(), ColourHeritage.From(rgbLinear));
        }

        internal static RgbLinear ToRgbLinear(Rgb rgb, RgbConfiguration rgbConfig)
        {
            var rgbMatrix = Matrix.FromTriplet(rgb.Triplet);
            var rgbLinearMatrix = rgbMatrix.Select(rgbConfig.InverseCompandToLinear);
            return new RgbLinear(rgbLinearMatrix.ToTriplet(), ColourHeritage.From(rgb));
        }
    }
}