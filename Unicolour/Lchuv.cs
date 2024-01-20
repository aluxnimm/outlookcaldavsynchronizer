namespace Wacton.Unicolour
{

    using static Utils;

    public record Lchuv : ColourRepresentation
    {
        protected override int? HueIndex => 2;
        public double L => First;
        public double C => Second;
        public double H => Third;
        public double ConstrainedH => ConstrainedThird;
        protected override double ConstrainedThird => H.Modulo(360.0);
        internal override bool IsGreyscale => L is <= 0.0 or >= 100.0 || C <= 0.0;

        public Lchuv(double l, double c, double h) : this(l, c, h, ColourHeritage.None)
        {
        }

        internal Lchuv(double l, double c, double h, ColourHeritage heritage) : base(l, c, h, heritage)
        {
        }

        protected override string FirstString => $"{L:F2}";
        protected override string SecondString => $"{C:F2}";
        protected override string ThirdString => UseAsHued ? $"{H:F1}°" : "—°";
        public override string ToString() => base.ToString();

        /*
         * LCHUV is a transform of LUV
         * Forward: https://en.wikipedia.org/wiki/CIELAB_color_space#CIEHLC_cylindrical_model
         * Reverse: https://en.wikipedia.org/wiki/CIELAB_color_space#CIEHLC_cylindrical_model
         */

        internal static Lchuv FromLuv(Luv luv)
        {
            var (l, c, h) = ToLchTriplet(luv.L, luv.U, luv.V);
            return new Lchuv(l, c, h, ColourHeritage.From(luv));
        }

        internal static Luv ToLuv(Lchuv lchuv)
        {
            var (l, u, v) = FromLchTriplet(lchuv.ConstrainedTriplet);
            return new Luv(l, u, v, ColourHeritage.From(lchuv));
        }
    }
}