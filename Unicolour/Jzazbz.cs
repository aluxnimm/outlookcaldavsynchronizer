using System;

namespace Wacton.Unicolour
{

    public record Jzazbz : ColourRepresentation
    {
        protected override int? HueIndex => null;
        public double J => First;
        public double A => Second;
        public double B => Third;

        // based on the figures from the paper, greyscale behaviour is the same as LAB
        // i.e. non-lightness axes are zero
        // but no clear lightness upper-bound
        internal override bool IsGreyscale => J <= 0.0 || (A.Equals(0.0) && B.Equals(0.0));

        public Jzazbz(double j, double a, double b) : this(j, a, b, ColourHeritage.None)
        {
        }

        internal Jzazbz(double j, double a, double b, ColourHeritage heritage) : base(j, a, b, heritage)
        {
        }

        protected override string FirstString => $"{J:F3}";
        protected override string SecondString => $"{A:+0.000;-0.000;0.000}";
        protected override string ThirdString => $"{B:+0.000;-0.000;0.000}";
        public override string ToString() => base.ToString();

        /*
         * JZAZBZ is a transform of XYZ
         * Forward: https://doi.org/10.1364/OE.25.015131
         * Reverse: https://doi.org/10.1364/OE.25.015131
         * -------
         * also useful: https://opticapublishing.figshare.com/articles/software/JzAzBz_m/5016299
         */

        // ReSharper disable InconsistentNaming
        private const double b = 1.15;
        private const double g = 0.66;
        private const double d = -0.56;
        private const double d0 = 1.6295499532821566e-11;
        // ReSharper restore InconsistentNaming

        private static readonly Matrix M1 = new(new[,]
        {
            { +0.41478972, +0.579999, +0.0146480 },
            { -0.2015100, +1.120649, +0.0531008 },
            { -0.0166008, +0.264800, +0.6684799 }
        });

        private static readonly Matrix M2 = new(new[,]
        {
            { +0.5, +0.5, 0 },
            { +3.524000, -4.066708, +0.542708 },
            { +0.199076, +1.096799, -1.295875 }
        });

        internal static Jzazbz FromXyz(Xyz xyz, double jzazbzScalar, XyzConfiguration xyzConfig)
        {
            var xyzMatrix = Matrix.FromTriplet(xyz.Triplet);
            var d65Matrix = Adaptation.WhitePoint(xyzMatrix, xyzConfig.WhitePoint, WhitePoint.From(Illuminant.D65));
            var d65ScaledMatrix = d65Matrix.Scale(jzazbzScalar).Select(x => Math.Max(x, 0));
            var (x65, y65, z65) = d65ScaledMatrix.ToTriplet();

            var x65Prime = b * x65 - (b - 1) * z65;
            var y65Prime = g * y65 - (g - 1) * x65;
            var xyz65PrimeMatrix = Matrix.FromTriplet(x65Prime, y65Prime, z65);
            var lmsMatrix = M1.Multiply(xyz65PrimeMatrix);
            var lmsPrimeMatrix = lmsMatrix.Select(Pq.Jzazbz.InverseEotf);
            var izazbzMatrix = M2.Multiply(lmsPrimeMatrix);

            var (iz, az, bz) = izazbzMatrix.ToTriplet();
            var jz = (1 + d) * iz / (1 + d * iz) - d0;
            return new Jzazbz(jz, az, bz, ColourHeritage.From(xyz));
        }

        internal static Xyz ToXyz(Jzazbz jzazbz, double jzazbzScalar, XyzConfiguration xyzConfig)
        {
            var (jz, az, bz) = jzazbz.Triplet;
            var iz = (jz + d0) / (1 + d - d * (jz + d0));
            var izazbzMatrix = Matrix.FromTriplet(iz, az, bz);
            var lmsPrimeMatrix = M2.Inverse().Multiply(izazbzMatrix);
            var lmsMatrix = lmsPrimeMatrix.Select(Pq.Jzazbz.Eotf);
            var xyz65PrimeMatrix = M1.Inverse().Multiply(lmsMatrix);
            var (x65Prime, y65Prime, z65Prime) = xyz65PrimeMatrix.ToTriplet();

            var x65 = (x65Prime + (b - 1) * z65Prime) / b;
            var y65 = (y65Prime + (g - 1) * x65) / g;
            var z65 = z65Prime;
            var d65ScaledMatrix = Matrix.FromTriplet(x65, y65, z65);
            var d65Matrix = d65ScaledMatrix.Scale(1 / jzazbzScalar);
            var xyzMatrix = Adaptation.WhitePoint(d65Matrix, WhitePoint.From(Illuminant.D65), xyzConfig.WhitePoint);
            return new Xyz(xyzMatrix.ToTriplet(), ColourHeritage.From(jzazbz));
        }
    }
}