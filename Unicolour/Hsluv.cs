namespace Wacton.Unicolour
{

    public record Hsluv : ColourRepresentation
    {
        protected override int? HueIndex => 0;
        public double H => First;
        public double S => Second;
        public double L => Third;
        public double ConstrainedH => ConstrainedFirst;
        public double ConstrainedS => ConstrainedSecond;
        public double ConstrainedL => ConstrainedThird;
        protected override double ConstrainedFirst => H.Modulo(360.0);
        protected override double ConstrainedSecond => S.Clamp(0.0, 100.0);
        protected override double ConstrainedThird => L.Clamp(0.0, 100.0);
        internal override bool IsGreyscale => S <= 0.0 || L is <= 0.0 or >= 100.0;

        public Hsluv(double h, double s, double l) : this(h, s, l, ColourHeritage.None)
        {
        }

        internal Hsluv(double h, double s, double l, ColourHeritage heritage) : base(h, s, l, heritage)
        {
        }

        protected override string FirstString => UseAsHued ? $"{H:F1}°" : "—°";
        protected override string SecondString => $"{S:F1}%";
        protected override string ThirdString => $"{L:F1}%";
        public override string ToString() => base.ToString();

        /*
         * HSLUV is a transform of LCHUV
         * Forward: https://github.com/hsluv/hsluv-haxe/blob/master/src/hsluv/Hsluv.hx#L363
         * Reverse: https://github.com/hsluv/hsluv-haxe/blob/master/src/hsluv/Hsluv.hx#L346
         */

        internal static Hsluv FromLchuv(Lchuv lchuv)
        {
            var (lchLightness, chroma, hue) = lchuv.ConstrainedTriplet;
            double saturation;
            double lightness;

            switch (lchLightness)
            {
                case > 99.9999999:
                    saturation = 0.0;
                    lightness = 100.0;
                    break;
                case < 0.00000001:
                    saturation = 0.0;
                    lightness = 0.0;
                    break;
                default:
                {
                    var maxChroma = BoundingLines.CalculateMaxChroma(lchLightness, hue);
                    saturation = chroma / maxChroma * 100;
                    lightness = lchLightness;
                    break;
                }
            }

            return new Hsluv(hue, saturation, lightness, ColourHeritage.From(lchuv));
        }

        internal static Lchuv ToLchuv(Hsluv hsluv)
        {
            var hue = hsluv.ConstrainedH;
            var saturation = hsluv.S;
            var hslLightness = hsluv.L;
            double lightness;
            double chroma;

            switch (hslLightness)
            {
                case > 99.9999999:
                    lightness = 100.0;
                    chroma = 0.0;
                    break;
                case < 0.00000001:
                    lightness = 0.0;
                    chroma = 0.0;
                    break;
                default:
                {
                    var maxChroma = BoundingLines.CalculateMaxChroma(hslLightness, hue);
                    chroma = maxChroma / 100 * saturation;
                    lightness = hslLightness;
                    break;
                }
            }

            return new Lchuv(lightness, chroma, hue, ColourHeritage.From(hsluv));
        }
    }
}