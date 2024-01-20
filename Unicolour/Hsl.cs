using System;

namespace Wacton.Unicolour
{

    public record Hsl : ColourRepresentation
    {
        protected override int? HueIndex => 0;
        public double H => First;
        public double S => Second;
        public double L => Third;
        public double ConstrainedH => ConstrainedFirst;
        public double ConstrainedS => ConstrainedSecond;
        public double ConstrainedL => ConstrainedThird;
        protected override double ConstrainedFirst => H.Modulo(360.0);
        protected override double ConstrainedSecond => S.Clamp(0.0, 1.0);
        protected override double ConstrainedThird => L.Clamp(0.0, 1.0);
        internal override bool IsGreyscale => S <= 0.0 || L is <= 0.0 or >= 1.0;

        public Hsl(double h, double s, double l) : this(h, s, l, ColourHeritage.None)
        {
        }

        internal Hsl(double h, double s, double l, ColourHeritage heritage) : base(h, s, l, heritage)
        {
        }

        protected override string FirstString => UseAsHued ? $"{H:F1}°" : "—°";
        protected override string SecondString => $"{S * 100:F1}%";
        protected override string ThirdString => $"{L * 100:F1}%";
        public override string ToString() => base.ToString();

        /*
         * HSL is a transform of HSB (in terms of Unicolour implementation)
         * Forward: https://en.wikipedia.org/wiki/HSL_and_HSV#HSV_to_HSL
         * Reverse: https://en.wikipedia.org/wiki/HSL_and_HSV#HSL_to_HSV
         */

        internal static Hsl FromHsb(Hsb hsb)
        {
            var (hue, hsbSaturation, brightness) = hsb.ConstrainedTriplet;
            var lightness = brightness * (1 - hsbSaturation / 2);
            var saturation = lightness is > 0.0 and < 1.0
                ? (brightness - lightness) / Math.Min(lightness, 1 - lightness)
                : 0;

            return new Hsl(hue, saturation, lightness, ColourHeritage.From(hsb));
        }

        internal static Hsb ToHsb(Hsl hsl)
        {
            var (hue, hslSaturation, lightness) = hsl.ConstrainedTriplet;
            var brightness = lightness + hslSaturation * Math.Min(lightness, 1 - lightness);
            var saturation = brightness > 0.0
                ? 2 * (1 - lightness / brightness)
                : 0;

            return new Hsb(hue, saturation, brightness, ColourHeritage.From(hsl));
        }
    }
}