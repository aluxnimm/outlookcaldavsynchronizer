namespace Wacton.Unicolour
{

    public record Ictcp : ColourRepresentation
    {
        protected override int? HueIndex => null;
        public double I => First;
        public double Ct => Second;
        public double Cp => Third;

        // no clear lightness upper-bound
        internal override bool IsGreyscale => I <= 0.0 || (Ct.Equals(0.0) && Cp.Equals(0.0));

        public Ictcp(double i, double ct, double cp) : this(i, ct, cp, ColourHeritage.None)
        {
        }

        internal Ictcp(ColourTriplet triplet, ColourHeritage heritage) : this(triplet.First, triplet.Second,
            triplet.Third, heritage)
        {
        }

        internal Ictcp(double i, double ct, double cp, ColourHeritage heritage) : base(i, ct, cp, heritage)
        {
        }

        protected override string FirstString => $"{I:F2}";
        protected override string SecondString => $"{Ct:+0.00;-0.00;0.00}";
        protected override string ThirdString => $"{Cp:+0.00;-0.00;0.00}";
        public override string ToString() => base.ToString();

        /*
         * ICTCP is a transform of XYZ
         * Forward: https://professional.dolby.com/siteassets/pdfs/dolby-vision-measuring-perceptual-color-volume-v7.1.pdf
         * Reverse: not specified in the above paper; implementation unit tested to confirm roundtrip conversion
         * -------
         * currently only support PQ transfer function, not HLG (https://en.wikipedia.org/wiki/Hybrid_log%E2%80%93gamma)
         */

        private static readonly Matrix M1 = new(new[,]
        {
            { 0.3593, 0.6976, -0.0359 },
            { -0.1921, 1.1005, 0.0754 },
            { 0.0071, 0.0748, 0.8433 }
        });

        private static readonly Matrix M2 = new Matrix(new double[,]
        {
            { 2048, 2048, 0 },
            { 6610, -13613, 7003 },
            { 17933, -17390, -543 }
        }).Scale(1 / 4096.0);

        internal static Ictcp FromXyz(Xyz xyz, double ictcpScalar, XyzConfiguration xyzConfig)
        {
            var xyzMatrix = Matrix.FromTriplet(xyz.Triplet);
            var d65Matrix = Adaptation.WhitePoint(xyzMatrix, xyzConfig.WhitePoint, WhitePoint.From(Illuminant.D65));
            var d65ScaledMatrix = d65Matrix.Scale(ictcpScalar);
            var lmsMatrix = M1.Multiply(d65ScaledMatrix);
            var lmsPrimeMatrix = lmsMatrix.Select(Pq.Smpte.InverseEotf);
            var ictcpMatrix = M2.Multiply(lmsPrimeMatrix);
            return new Ictcp(ictcpMatrix.ToTriplet(), ColourHeritage.From(xyz));
        }

        internal static Xyz ToXyz(Ictcp ictcp, double ictcpScalar, XyzConfiguration xyzConfig)
        {
            var ictcpMatrix = Matrix.FromTriplet(ictcp.Triplet);
            var lmsPrimeMatrix = M2.Inverse().Multiply(ictcpMatrix);
            var lmsMatrix = lmsPrimeMatrix.Select(Pq.Smpte.Eotf);
            var d65ScaledMatrix = lmsMatrix.Scale(1 / ictcpScalar);
            var d65Matrix = M1.Inverse().Multiply(d65ScaledMatrix);
            var xyzMatrix = Adaptation.WhitePoint(d65Matrix, WhitePoint.From(Illuminant.D65), xyzConfig.WhitePoint);
            return new Xyz(xyzMatrix.ToTriplet(), ColourHeritage.From(ictcp));
        }
    }
}