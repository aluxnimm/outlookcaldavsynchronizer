namespace Wacton.Unicolour
{

    public record Rgb255 : ColourRepresentation
    {
        protected override int? HueIndex => null;
        public int R => (int)First;
        public int G => (int)Second;
        public int B => (int)Third;
        public int ConstrainedR => (int)ConstrainedFirst;
        public int ConstrainedG => (int)ConstrainedSecond;
        public int ConstrainedB => (int)ConstrainedThird;
        protected override double ConstrainedFirst => R.Clamp(0, 255);
        protected override double ConstrainedSecond => G.Clamp(0, 255);
        protected override double ConstrainedThird => B.Clamp(0, 255);
        internal override bool IsGreyscale => ConstrainedR.Equals(ConstrainedG) && ConstrainedG.Equals(ConstrainedB);

        public string ConstrainedHex => UseAsNaN ? "-" : $"#{ConstrainedR:X2}{ConstrainedG:X2}{ConstrainedB:X2}";

        public Rgb255(double r, double g, double b) : this(r, g, b, ColourHeritage.None)
        {
        }

        internal Rgb255(double r, double g, double b, ColourHeritage heritage) : base(r, g, b, heritage)
        {
        }

        protected override string FirstString => $"{R}";
        protected override string SecondString => $"{G}";
        protected override string ThirdString => $"{B}";
        public override string ToString() => base.ToString();
    }
}