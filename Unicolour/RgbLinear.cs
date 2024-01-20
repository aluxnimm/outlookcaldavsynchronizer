namespace Wacton.Unicolour
{

    public record RgbLinear : ColourRepresentation
    {
        protected override int? HueIndex => null;
        public double R => First;
        public double G => Second;
        public double B => Third;
        public double ConstrainedR => ConstrainedFirst;
        public double ConstrainedG => ConstrainedSecond;
        public double ConstrainedB => ConstrainedThird;
        protected override double ConstrainedFirst => R.Clamp(0.0, 1.0);
        protected override double ConstrainedSecond => G.Clamp(0.0, 1.0);
        protected override double ConstrainedThird => B.Clamp(0.0, 1.0);
        internal override bool IsGreyscale => ConstrainedR.Equals(ConstrainedG) && ConstrainedG.Equals(ConstrainedB);

        // https://www.w3.org/TR/WCAG21/#dfn-relative-luminance - effectively an approximation of Y from XYZ, but will stick to the specification
        internal double RelativeLuminance => UseAsNaN ? double.NaN : 0.2126 * R + 0.7152 * G + 0.0722 * B;

        public RgbLinear(double r, double g, double b) : this(r, g, b, ColourHeritage.None)
        {
        }

        internal RgbLinear(ColourTriplet triplet, ColourHeritage heritage) : this(triplet.First, triplet.Second,
            triplet.Third, heritage)
        {
        }

        internal RgbLinear(double r, double g, double b, ColourHeritage heritage) : base(r, g, b, heritage)
        {
        }

        protected override string FirstString => $"{R:F2}";
        protected override string SecondString => $"{G:F2}";
        protected override string ThirdString => $"{B:F2}";
        public override string ToString() => base.ToString();

        /*
         * RGB Linear is a transform of XYZ
         * Forward: https://en.wikipedia.org/wiki/SRGB#From_CIE_XYZ_to_sRGB
         * Reverse: https://en.wikipedia.org/wiki/SRGB#From_sRGB_to_CIE_XYZ
         */

        internal static RgbLinear FromXyz(Xyz xyz, RgbConfiguration rgbConfig, XyzConfiguration xyzConfig)
        {
            var xyzMatrix = Matrix.FromTriplet(xyz.Triplet);
            var transformationMatrix = RgbLinearToXyzMatrix(rgbConfig, xyzConfig).Inverse();
            var rgbLinearMatrix = transformationMatrix.Multiply(xyzMatrix);
            return new RgbLinear(rgbLinearMatrix.ToTriplet(), ColourHeritage.From(xyz));
        }

        internal static Xyz ToXyz(RgbLinear rgbLinear, RgbConfiguration rgbConfig, XyzConfiguration xyzConfig)
        {
            var rgbLinearMatrix = Matrix.FromTriplet(rgbLinear.Triplet);
            var transformationMatrix = RgbLinearToXyzMatrix(rgbConfig, xyzConfig);
            var xyzMatrix = transformationMatrix.Multiply(rgbLinearMatrix);
            return new Xyz(xyzMatrix.ToTriplet(), ColourHeritage.From(rgbLinear));
        }

        // http://www.brucelindbloom.com/index.html?Eqn_RGB_XYZ_Matrix.html
        internal static Matrix RgbLinearToXyzMatrix(RgbConfiguration rgbConfig, XyzConfiguration xyzConfig)
        {
            var cr = rgbConfig.ChromaticityR;
            var cg = rgbConfig.ChromaticityG;
            var cb = rgbConfig.ChromaticityB;

            double X(Chromaticity c) => c.X / c.Y;
            double Y(Chromaticity c) => 1;
            double Z(Chromaticity c) => (1 - c.X - c.Y) / c.Y;

            var (xr, yr, zr) = (X(cr), Y(cr), Z(cr));
            var (xg, yg, zg) = (X(cg), Y(cg), Z(cg));
            var (xb, yb, zb) = (X(cb), Y(cb), Z(cb));

            var fromPrimaries = new Matrix(new[,]
            {
                { xr, xg, xb },
                { yr, yg, yb },
                { zr, zg, zb }
            });

            var sourceWhite = rgbConfig.WhitePoint.AsXyzMatrix();
            var (sr, sg, sb) = fromPrimaries.Inverse().Multiply(sourceWhite).ToTriplet();

            var matrix = new Matrix(new[,]
            {
                { sr * xr, sg * xg, sb * xb },
                { sr * yr, sg * yg, sb * yb },
                { sr * zr, sg * zg, sb * zb }
            });

            var adaptedMatrix = Adaptation.WhitePoint(matrix, rgbConfig.WhitePoint, xyzConfig.WhitePoint);
            return adaptedMatrix;
        }
    }
}