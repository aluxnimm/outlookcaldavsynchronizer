namespace Wacton.Unicolour
{

    public record Hwb : ColourRepresentation
    {
        protected override int? HueIndex => 0;
        public double H => First;
        public double W => Second;
        public double B => Third;
        public double ConstrainedH => ConstrainedFirst;
        public double ConstrainedW => ConstrainedSecond;
        public double ConstrainedB => ConstrainedThird;
        protected override double ConstrainedFirst => H.Modulo(360.0);
        protected override double ConstrainedSecond => W.Clamp(0.0, 1.0);
        protected override double ConstrainedThird => B.Clamp(0.0, 1.0);
        internal override bool IsGreyscale => ConstrainedW + ConstrainedB >= 1.0;

        public Hwb(double h, double w, double b) : this(h, w, b, ColourHeritage.None)
        {
        }

        internal Hwb(double h, double w, double b, ColourHeritage heritage) : base(h, w, b, heritage)
        {
        }

        protected override string FirstString => UseAsHued ? $"{H:F1}°" : "—°";
        protected override string SecondString => $"{W * 100:F1}%";
        protected override string ThirdString => $"{B * 100:F1}%";
        public override string ToString() => base.ToString();

        /*
         * HWB is a transform of HSB (in terms of Unicolour implementation)
         * Forward: https://en.wikipedia.org/wiki/HWB_color_model#Conversion
         * Reverse: https://en.wikipedia.org/wiki/HWB_color_model#Conversion
         */

        internal static Hwb FromHsb(Hsb hsb)
        {
            var (hue, s, b) = hsb.ConstrainedTriplet;
            var whiteness = (1 - s) * b;
            var blackness = 1 - b;
            return new Hwb(hue, whiteness, blackness, ColourHeritage.From(hsb));
        }

        internal static Hsb ToHsb(Hwb hwb)
        {
            var (hue, w, b) = hwb.ConstrainedTriplet;

            double brightness;
            double saturation;
            if (hwb.IsGreyscale)
            {
                brightness = w / (w + b);
                saturation = 0;
            }
            else
            {
                brightness = 1 - b;
                saturation = brightness == 0.0 ? 0 : 1 - w / brightness;
            }

            return new Hsb(hue, saturation, brightness, ColourHeritage.From(hwb));
        }
    }
}