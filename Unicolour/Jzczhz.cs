namespace Wacton.Unicolour
{

    using static Utils;

    public record Jzczhz : ColourRepresentation
    {
        protected override int? HueIndex => 2;
        public double J => First;
        public double C => Second;
        public double H => Third;
        public double ConstrainedH => ConstrainedThird;
        protected override double ConstrainedThird => H.Modulo(360.0);

        // no clear lightness upper-bound
        // (paper says lightness J is 0 - 1 but seems like it's a scaling of their plot of Rec.2020 gamut - in my tests maxes out after ~0.17)
        internal override bool IsGreyscale => J <= 0.0 || C <= 0.0;

        public Jzczhz(double j, double c, double h) : this(j, c, h, ColourHeritage.None)
        {
        }

        internal Jzczhz(double j, double c, double h, ColourHeritage heritage) : base(j, c, h, heritage)
        {
        }

        protected override string FirstString => $"{J:F3}";
        protected override string SecondString => $"{C:F3}";
        protected override string ThirdString => UseAsHued ? $"{H:F1}°" : "—°";
        public override string ToString() => base.ToString();

        /*
         * JZCZHZ is a transform of JZAZBZ
         * Forward: https://en.wikipedia.org/wiki/CIELAB_color_space#CIEHLC_cylindrical_model
         * Reverse: https://en.wikipedia.org/wiki/CIELAB_color_space#CIEHLC_cylindrical_model
         */

        internal static Jzczhz FromJzazbz(Jzazbz jzazbz)
        {
            var (jz, cz, hz) = ToLchTriplet(jzazbz.J, jzazbz.A, jzazbz.B);
            return new Jzczhz(jz, cz, hz, ColourHeritage.From(jzazbz));
        }

        internal static Jzazbz ToJzazbz(Jzczhz jzczhz)
        {
            var (jz, az, bz) = FromLchTriplet(jzczhz.ConstrainedTriplet);
            return new Jzazbz(jz, az, bz, ColourHeritage.From(jzczhz));
        }
    }
}