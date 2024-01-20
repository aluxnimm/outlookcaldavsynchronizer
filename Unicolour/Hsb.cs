using System;
using System.Linq;

namespace Wacton.Unicolour
{

    public record Hsb : ColourRepresentation
    {
        protected override int? HueIndex => 0;
        public double H => First;
        public double S => Second;
        public double B => Third;
        public double ConstrainedH => ConstrainedFirst;
        public double ConstrainedS => ConstrainedSecond;
        public double ConstrainedB => ConstrainedThird;
        protected override double ConstrainedFirst => H.Modulo(360.0);
        protected override double ConstrainedSecond => S.Clamp(0.0, 1.0);
        protected override double ConstrainedThird => B.Clamp(0.0, 1.0);
        internal override bool IsGreyscale => S <= 0.0 || B <= 0.0;

        public Hsb(double h, double s, double b) : this(h, s, b, ColourHeritage.None)
        {
        }

        internal Hsb(double h, double s, double b, ColourHeritage heritage) : base(h, s, b, heritage)
        {
        }

        protected override string FirstString => UseAsHued ? $"{H:F1}°" : "—°";
        protected override string SecondString => $"{S * 100:F1}%";
        protected override string ThirdString => $"{B * 100:F1}%";
        public override string ToString() => base.ToString();

        /*
         * HSB is a transform of RGB
         * Forward: https://en.wikipedia.org/wiki/HSL_and_HSV#From_RGB
         * Reverse: https://en.wikipedia.org/wiki/HSL_and_HSV#HSV_to_RGB
         */

        internal static Hsb FromRgb(Rgb rgb)
        {
            var (r, g, b) = rgb.ConstrainedTriplet;
            var components = new[] { r, g, b };
            var xMax = components.Max();
            var xMin = components.Min();
            var chroma = xMax - xMin;

            double hue;
            if (chroma == 0.0) hue = 0;
            else if (xMax == r) hue = 60 * (0 + (g - b) / chroma);
            else if (xMax == g) hue = 60 * (2 + (b - r) / chroma);
            else if (xMax == b) hue = 60 * (4 + (r - g) / chroma);
            else hue = double.NaN;
            var brightness = xMax;
            var saturation = brightness == 0 ? 0 : chroma / brightness;
            return new Hsb(hue.Modulo(360.0), saturation, brightness, ColourHeritage.From(rgb));
        }

        internal static Rgb ToRgb(Hsb hsb)
        {
            var (hue, saturation, brightness) = hsb.ConstrainedTriplet;
            var chroma = brightness * saturation;
            var h = hue / 60;
            var x = chroma * (1 - Math.Abs(h % 2 - 1));

            var (r, g, b) = h switch
            {
                < 1 => (chroma, x, 0.0),
                < 2 => (x, chroma, 0.0),
                < 3 => (0.0, chroma, x),
                < 4 => (0.0, x, chroma),
                < 5 => (x, 0.0, chroma),
                < 6 => (chroma, 0.0, x),
                _ => (0.0, 0.0, 0.0)
            };

            var m = brightness - chroma;
            var (red, green, blue) = (r + m, g + m, b + m);
            return new Rgb(red, green, blue, ColourHeritage.From(hsb));
        }
    }
}