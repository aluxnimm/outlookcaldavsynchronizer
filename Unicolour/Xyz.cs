namespace Wacton.Unicolour
{

    public record Xyz : ColourRepresentation
    {
        protected override int? HueIndex => null;
        public double X => First;
        public double Y => Second;
        public double Z => Third;

        // no clear luminance upper-bound; usually Y >= 1 is max luminance
        // but since custom white points can be provided, don't want to make the assumption
        internal override bool IsGreyscale => Y <= 0;

        public Xyz(double x, double y, double z) : this(x, y, z, ColourHeritage.None)
        {
        }

        internal Xyz(ColourTriplet triplet, ColourHeritage heritage) : this(triplet.First, triplet.Second,
            triplet.Third, heritage)
        {
        }

        internal Xyz(double x, double y, double z, ColourHeritage heritage) : base(x, y, z, heritage)
        {
        }

        protected override string FirstString => $"{X:F4}";
        protected override string SecondString => $"{Y:F4}";
        protected override string ThirdString => $"{Z:F4}";
        public override string ToString() => base.ToString();

        /*
         * XYZ is considered the root colour representation (in terms of Unicolour implementation)
         * so does not contain any forward (from another space) or reverse (back to original space) functions
         */

        // only for potential debugging or diagnostics
        // until there is an "official" HCT -> XYZ reverse transform
        internal HctToXyzSearchResult? HctToXyzSearchResult;
    }
}