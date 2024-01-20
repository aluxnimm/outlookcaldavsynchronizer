using System;

namespace Wacton.Unicolour
{
    using static Utils;

    internal static class Comparison
    {
        // https://www.w3.org/WAI/WCAG21/Techniques/general/G18.html#tests
        // minimal recommended contrast ratio is 4.5, or 3 for larger font-sizes
        internal static double Contrast(Unicolour colour1, Unicolour colour2)
        {
            var luminance1 = colour1.RelativeLuminance;
            var luminance2 = colour2.RelativeLuminance;
            var l1 = Math.Max(luminance1, luminance2); // lighter of the colours
            var l2 = Math.Min(luminance1, luminance2); // darker of the colours
            return (l1 + 0.05) / (l2 + 0.05);
        }

        internal static double Difference(DeltaE deltaE, Unicolour reference, Unicolour sample)
        {
            return deltaE switch
            {
                DeltaE.Cie76 => DeltaE76(reference, sample),
                DeltaE.Cie94 => DeltaE94(reference, sample),
                DeltaE.Cie94Textiles => DeltaE94(reference, sample, isForTextiles: true),
                DeltaE.Ciede2000 => DeltaE00(reference, sample),
                DeltaE.CmcAcceptability => DeltaECmc(reference, sample, l: 1, c: 1),
                DeltaE.CmcPerceptibility => DeltaECmc(reference, sample, l: 2, c: 1),
                DeltaE.Itp => DeltaEItp(reference, sample),
                DeltaE.Z => DeltaEz(reference, sample),
                DeltaE.Hyab => DeltaEHyab(reference, sample),
                DeltaE.Ok => DeltaEOk(reference, sample),
                DeltaE.Cam02 => DeltaECam02(reference, sample),
                DeltaE.Cam16 => DeltaECam16(reference, sample),
                _ => throw new ArgumentOutOfRangeException(nameof(deltaE), deltaE, null)
            };
        }

        // https://en.wikipedia.org/wiki/Color_difference#CIE76
        private static double DeltaE76(Unicolour reference, Unicolour sample)
        {
            var (l1, a1, b1) = reference.Lab.Triplet;
            var (l2, a2, b2) = sample.Lab.Triplet;
            return Math.Sqrt(SquaredDiff(l1, l2) + SquaredDiff(a1, a2) + SquaredDiff(b1, b2));
        }

        // https://en.wikipedia.org/wiki/Color_difference#CIE94
        private static double DeltaE94(Unicolour reference, Unicolour sample, bool isForTextiles = false)
        {
            var (l1, a1, b1) = reference.Lab.Triplet;
            var (l2, a2, b2) = sample.Lab.Triplet;
            var (_, c1, _) = reference.Lchab.Triplet;
            var (_, c2, _) = sample.Lchab.Triplet;

            var lDelta = l1 - l2;
            var aDelta = a1 - a2;
            var bDelta = b1 - b2;
            var cDelta = c1 - c2;
            var hDelta = Math.Sqrt(
                Math.Pow(aDelta, 2) +
                Math.Pow(bDelta, 2) -
                Math.Pow(cDelta, 2)
            );

            var k1 = isForTextiles ? 0.048 : 0.045;
            var k2 = isForTextiles ? 0.014 : 0.015;
            var kl = isForTextiles ? 2 : 1;
            const int kc = 1;
            const int kh = 1;

            const int sl = 1;
            var sc = 1 + k1 * c1;
            var sh = 1 + k2 * c1;

            return Math.Sqrt(
                Math.Pow(lDelta / (kl * sl), 2) +
                Math.Pow(cDelta / (kc * sc), 2) +
                Math.Pow(hDelta / (kh * sh), 2)
            );
        }

        // https://en.wikipedia.org/wiki/Color_difference#CIEDE2000
        private static double DeltaE00(Unicolour reference, Unicolour sample)
        {
            var (l1, a1, b1) = reference.Lab.Triplet;
            var (l2, a2, b2) = sample.Lab.Triplet;
            var (_, c1, _) = reference.Lchab.Triplet;
            var (_, c2, _) = sample.Lchab.Triplet;

            double Power2(double value) => Math.Pow(value, 2);
            double Power7(double value) => Math.Pow(value, 7);
            double SqrtCPower7(double c) => Math.Sqrt(Power7(c) / (Power7(c) + Power7(25)));

            var avgL = (l1 + l2) / 2.0;
            var avgC = (c1 + c2) / 2.0;
            var g = 0.5 * (1 - SqrtCPower7(avgC));
            var a1Prime = a1 * (1 + g);
            var a2Prime = a2 * (1 + g);
            var c1Prime = Math.Sqrt(Power2(a1Prime) + Power2(b1));
            var c2Prime = Math.Sqrt(Power2(a2Prime) + Power2(b2));
            var avgCPrime = (c1Prime + c2Prime) / 2.0;
            var h1Prime = ToDegrees(Math.Atan2(b1, a1Prime)).Modulo(360);
            var h2Prime = ToDegrees(Math.Atan2(b2, a2Prime)).Modulo(360);

            var hPrimeDelta = Math.Abs(h1Prime - h2Prime) switch
            {
                <= 180 => h2Prime - h1Prime,
                > 180 when h2Prime <= h1Prime => h2Prime - h1Prime + 360,
                > 180 when h2Prime > h1Prime => h2Prime - h1Prime - 360,
                _ => double.NaN
            };

            var avgHPrime = Math.Abs(h1Prime - h2Prime) switch
            {
                <= 180 => (h1Prime + h2Prime) / 2.0,
                > 180 when h1Prime + h2Prime < 360 => (h1Prime + h2Prime + 360) / 2.0,
                > 180 when h1Prime + h2Prime >= 360 => (h1Prime + h2Prime - 360) / 2.0,
                _ => double.NaN
            };

            var t = 1 -
                    0.17 * Math.Cos(ToRadians(avgHPrime - 30)) +
                    0.24 * Math.Cos(ToRadians(2 * avgHPrime)) +
                    0.32 * Math.Cos(ToRadians(3 * avgHPrime + 6)) -
                    0.20 * Math.Cos(ToRadians(4 * avgHPrime - 63));

            var deltaLPrime = l2 - l1;
            var deltaCPrime = c2 - c1;
            var deltaHPrime = 2 * Math.Sqrt(c1Prime * c2Prime) * Math.Sin(ToRadians(hPrimeDelta / 2.0));

            const int kl = 1;
            const int kc = 1;
            const int kh = 1;

            var sl = 1 + (0.015 * Power2(avgL - 50)) / Math.Sqrt(20 + Power2(avgL - 50));
            var sc = 1 + 0.045 * avgCPrime;
            var sh = 1 + 0.015 * avgCPrime * t;

            var deltaTheta = 30 * Math.Exp(-Power2((avgHPrime - 275) / 25.0));
            var rc = 2 * SqrtCPower7(avgCPrime);
            var rt = -rc * Math.Sin(ToRadians(2 * deltaTheta));

            double Ratio(double delta, double k, double s) => delta / (k * s);
            var ratioL = Ratio(deltaLPrime, kl, sl);
            var ratioC = Ratio(deltaCPrime, kc, sc);
            var ratioH = Ratio(deltaHPrime, kh, sh);
            return Math.Sqrt(
                Power2(ratioL) +
                Power2(ratioC) +
                Power2(ratioH) +
                rt * ratioC * ratioH);
        }

        // https://en.wikipedia.org/wiki/Color_difference#CMC_l:c_(1984)
        private static double DeltaECmc(Unicolour reference, Unicolour sample, double l, double c)
        {
            var (l1, a1, b1) = reference.Lab.Triplet;
            var (l2, a2, b2) = sample.Lab.Triplet;
            var (_, c1, h1) = reference.Lchab.ConstrainedTriplet;
            var (_, c2, _) = sample.Lchab.Triplet;

            var lDelta = l1 - l2;
            var aDelta = a1 - a2;
            var bDelta = b1 - b2;
            var cDelta = c1 - c2;
            var hDelta = Math.Sqrt(
                Math.Pow(aDelta, 2) +
                Math.Pow(bDelta, 2) -
                Math.Pow(cDelta, 2)
            );

            var f = Math.Sqrt(Math.Pow(c1, 4) / (Math.Pow(c1, 4) + 1900));
            var t = h1 is > 165 and <= 345
                ? 0.56 + Math.Abs(0.2 * Math.Cos(ToRadians(h1 + 168)))
                : 0.36 + Math.Abs(0.4 * Math.Cos(ToRadians(h1 + 35)));

            var sl = l1 < 16 ? 0.511 : 0.040975 * l1 / (1 + 0.01765 * l1);
            var sc = 0.0638 * c1 / (1 + 0.0131 * c1) + 0.638;
            var sh = sc * (f * t + 1 - f);

            return Math.Sqrt(
                Math.Pow(lDelta / (l * sl), 2) +
                Math.Pow(cDelta / (c * sc), 2) +
                Math.Pow(hDelta / sh, 2)
            );
        }

        // https://www.itu.int/dms_pubrec/itu-r/rec/bt/R-REC-BT.2124-0-201901-I!!PDF-E.pdf
        private static double DeltaEItp(Unicolour reference, Unicolour sample)
        {
            ColourTriplet ToItp(Ictcp ictcp) => new(ictcp.I, 0.5 * ictcp.Ct, ictcp.Cp);
            var (i1, t1, c1) = ToItp(reference.Ictcp);
            var (i2, t2, c2) = ToItp(sample.Ictcp);
            return 720 * Math.Sqrt(SquaredDiff(i1, i2) + SquaredDiff(t1, t2) + SquaredDiff(c1, c2));
        }

        // https://doi.org/10.1364/OE.25.015131
        private static double DeltaEz(Unicolour reference, Unicolour sample)
        {
            var (jz1, cz1, hz1) = reference.Jzczhz.Triplet;
            var (jz2, cz2, hz2) = sample.Jzczhz.Triplet;

            var deltaHz = hz1 - hz2;
            deltaHz = 2 * Math.Sqrt(reference.Jzczhz.C * sample.Jzczhz.C) * Math.Sin(ToRadians(deltaHz / 2.0));
            deltaHz = Math.Pow(deltaHz, 2);
            return Math.Sqrt(SquaredDiff(jz1, jz2) + SquaredDiff(cz1, cz2) + deltaHz);
        }

        // https://en.wikipedia.org/wiki/Color_difference#Other_geometric_constructions
        private static double DeltaEHyab(Unicolour reference, Unicolour sample)
        {
            var (l1, a1, b1) = reference.Lab.Triplet;
            var (l2, a2, b2) = sample.Lab.Triplet;
            return Math.Sqrt(SquaredDiff(a1, a2) + SquaredDiff(b1, b2)) + Math.Abs(l2 - l1);
        }

        // https://www.w3.org/TR/css-color-4/#color-difference-OK
        private static double DeltaEOk(Unicolour reference, Unicolour sample)
        {
            var (l1, a1, b1) = reference.Oklab.Triplet;
            var (l2, a2, b2) = sample.Oklab.Triplet;
            return Math.Sqrt(SquaredDiff(l1, l2) + SquaredDiff(a1, a2) + SquaredDiff(b1, b2));
        }

        // https://doi.org/10.1007/978-1-4419-6190-7_2
        // currently only support UCS, not LCD or SCD - no need to handle ΔJ / kl since kl = 1
        private static double DeltaECam02(Unicolour reference, Unicolour sample)
        {
            var (j1, a1, b1) = reference.Cam02.Triplet;
            var (j2, a2, b2) = sample.Cam02.Triplet;
            return Math.Sqrt(SquaredDiff(j1, j2) + SquaredDiff(a1, a2) + SquaredDiff(b1, b2));
        }

        // https://doi.org/10.1002/col.22131
        private static double DeltaECam16(Unicolour reference, Unicolour sample)
        {
            var (j1, a1, b1) = reference.Cam16.Triplet;
            var (j2, a2, b2) = sample.Cam16.Triplet;
            var deltaE = Math.Sqrt(SquaredDiff(j1, j2) + SquaredDiff(a1, a2) + SquaredDiff(b1, b2));
            return 1.41 * Math.Pow(deltaE, 0.63);
        }

        private static double SquaredDiff(double first, double second) => Math.Pow(second - first, 2);
    }
}