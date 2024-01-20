using System;

namespace Wacton.Unicolour
{

    using static Utils;

    public record Oklab : ColourRepresentation
    {
        protected override int? HueIndex => null;
        public double L => First;
        public double A => Second;
        public double B => Third;
        internal override bool IsGreyscale => L is <= 0.0 or >= 1.0 || (A.Equals(0.0) && B.Equals(0.0));

        public Oklab(double l, double a, double b) : this(l, a, b, ColourHeritage.None)
        {
        }

        internal Oklab(ColourTriplet triplet, ColourHeritage heritage) : this(triplet.First, triplet.Second,
            triplet.Third, heritage)
        {
        }

        internal Oklab(double l, double a, double b, ColourHeritage heritage) : base(l, a, b, heritage)
        {
        }

        protected override string FirstString => $"{L:F2}";
        protected override string SecondString => $"{A:+0.00;-0.00;0.00}";
        protected override string ThirdString => $"{B:+0.00;-0.00;0.00}";
        public override string ToString() => base.ToString();

        /*
         * OKLAB is a transform of XYZ
         * Forward: https://bottosson.github.io/posts/oklab/#converting-from-xyz-to-oklab
         * Reverse: https://bottosson.github.io/posts/oklab/#converting-from-xyz-to-oklab
         */

        private static readonly Matrix M1 = new(new[,]
        {
            { +0.8189330101, +0.3618667424, -0.1288597137 },
            { +0.0329845436, +0.9293118715, +0.0361456387 },
            { +0.0482003018, +0.2643662691, +0.6338517070 }
        });

        private static readonly Matrix M2 = new(new[,]
        {
            { +0.2104542553, +0.7936177850, -0.0040720468 },
            { +1.9779984951, -2.4285922050, +0.4505937099 },
            { +0.0259040371, +0.7827717662, -0.8086757660 }
        });

        internal static Oklab FromXyz(Xyz xyz, XyzConfiguration xyzConfig)
        {
            var xyzMatrix = Matrix.FromTriplet(xyz.Triplet);
            var d65Matrix = Adaptation.WhitePoint(xyzMatrix, xyzConfig.WhitePoint, WhitePoint.From(Illuminant.D65));
            var lmsMatrix = M1.Multiply(d65Matrix);
            var lmsNonLinearMatrix = lmsMatrix.Select(CubeRoot);
            var labMatrix = M2.Multiply(lmsNonLinearMatrix);
            return new Oklab(labMatrix.ToTriplet(), ColourHeritage.From(xyz));
        }

        internal static Xyz ToXyz(Oklab oklab, XyzConfiguration xyzConfig)
        {
            var labMatrix = Matrix.FromTriplet(oklab.Triplet);
            var lmsNonLinearMatrix = M2.Inverse().Multiply(labMatrix);
            var lmsMatrix = lmsNonLinearMatrix.Select(x => Math.Pow(x, 3));
            var d65Matrix = M1.Inverse().Multiply(lmsMatrix);
            var xyzMatrix = Adaptation.WhitePoint(d65Matrix, WhitePoint.From(Illuminant.D65), xyzConfig.WhitePoint);
            return new Xyz(xyzMatrix.ToTriplet(), ColourHeritage.From(oklab));
        }
    }
}