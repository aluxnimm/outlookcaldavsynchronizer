namespace Wacton.Unicolour
{

    using static Utils;

    public record Oklch : ColourRepresentation
    {
        protected override int? HueIndex => 2;
        public double L => First;
        public double C => Second;
        public double H => Third;
        public double ConstrainedH => ConstrainedThird;
        protected override double ConstrainedThird => H.Modulo(360.0);
        internal override bool IsGreyscale => L is <= 0.0 or >= 1.0 || C <= 0.0;

        public Oklch(double l, double c, double h) : this(l, c, h, ColourHeritage.None)
        {
        }

        internal Oklch(double l, double c, double h, ColourHeritage heritage) : base(l, c, h, heritage)
        {
        }

        protected override string FirstString => $"{L:F2}";
        protected override string SecondString => $"{C:F2}";
        protected override string ThirdString => UseAsHued ? $"{H:F1}°" : "—°";
        public override string ToString() => base.ToString();

        /*
         * OKLCH is a transform of OKLAB
         * Forward: https://en.wikipedia.org/wiki/CIELAB_color_space#CIEHLC_cylindrical_model
         * Reverse: https://en.wikipedia.org/wiki/CIELAB_color_space#CIEHLC_cylindrical_model
         */

        internal static Oklch FromOklab(Oklab oklab)
        {
            var (l, c, h) = ToLchTriplet(oklab.L, oklab.A, oklab.B);
            return new Oklch(l, c, h, ColourHeritage.From(oklab));
        }

        internal static Oklab ToOklab(Oklch oklch)
        {
            var (l, a, b) = FromLchTriplet(oklch.ConstrainedTriplet);
            return new Oklab(l, a, b, ColourHeritage.From(oklch));
        }
    }
}