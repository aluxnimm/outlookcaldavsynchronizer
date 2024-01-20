namespace Wacton.Unicolour
{

    using static Utils;

    public record Lchab : ColourRepresentation
    {
        protected override int? HueIndex => 2;
        public double L => First;
        public double C => Second;
        public double H => Third;
        public double ConstrainedH => ConstrainedThird;
        protected override double ConstrainedThird => H.Modulo(360.0);
        internal override bool IsGreyscale => L is <= 0.0 or >= 100.0 || C <= 0.0;

        public Lchab(double l, double c, double h) : this(l, c, h, ColourHeritage.None)
        {
        }

        internal Lchab(double l, double c, double h, ColourHeritage heritage) : base(l, c, h, heritage)
        {
        }

        protected override string FirstString => $"{L:F2}";
        protected override string SecondString => $"{C:F2}";
        protected override string ThirdString => UseAsHued ? $"{H:F1}°" : "—°";
        public override string ToString() => base.ToString();

        /*
         * LCHAB is a transform of LAB
         * Forward: https://en.wikipedia.org/wiki/CIELAB_color_space#CIEHLC_cylindrical_model
         * Reverse: https://en.wikipedia.org/wiki/CIELAB_color_space#CIEHLC_cylindrical_model
         */

        internal static Lchab FromLab(Lab lab)
        {
            var (l, c, h) = ToLchTriplet(lab.L, lab.A, lab.B);
            return new Lchab(l, c, h, ColourHeritage.From(lab));
        }

        internal static Lab ToLab(Lchab lchab)
        {
            var (l, a, b) = FromLchTriplet(lchab.ConstrainedTriplet);
            return new Lab(l, a, b, ColourHeritage.From(lchab));
        }
    }
}