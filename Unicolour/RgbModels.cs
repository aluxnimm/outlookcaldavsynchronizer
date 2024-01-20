using System;

namespace Wacton.Unicolour
{

    public static class RgbModels
    {
        public static class StandardRgb
        {
            public static readonly Chromaticity R = new(0.6400, 0.3300);
            public static readonly Chromaticity G = new(0.3000, 0.6000);
            public static readonly Chromaticity B = new(0.1500, 0.0600);
            public static WhitePoint WhitePoint => WhitePoint.From(Illuminant.D65);

            public static double FromLinear(double linear)
            {
                return Companding.ReflectWhenNegative(linear, value =>
                    value <= 0.0031308
                        ? 12.92 * value
                        : 1.055 * Companding.Gamma(value, 2.4) - 0.055);
            }

            public static double ToLinear(double nonlinear)
            {
                return Companding.ReflectWhenNegative(nonlinear, value =>
                    value <= 0.04045
                        ? value / 12.92
                        : Companding.InverseGamma((value + 0.055) / 1.055, 2.4));
            }

            public static RgbConfiguration RgbConfiguration => new(R, G, B, WhitePoint, FromLinear, ToLinear);
        }

        public static class DisplayP3
        {
            public static readonly Chromaticity R = new(0.680, 0.320);
            public static readonly Chromaticity G = new(0.265, 0.690);
            public static readonly Chromaticity B = new(0.150, 0.060);
            public static WhitePoint WhitePoint => WhitePoint.From(Illuminant.D65);

            public static double FromLinear(double value) => StandardRgb.FromLinear(value);
            public static double ToLinear(double value) => StandardRgb.ToLinear(value);

            public static RgbConfiguration RgbConfiguration => new(R, G, B, WhitePoint, FromLinear, ToLinear);
        }

        public static class Rec2020
        {
            public static readonly Chromaticity R = new(0.708, 0.292);
            public static readonly Chromaticity G = new(0.170, 0.797);
            public static readonly Chromaticity B = new(0.131, 0.046);
            public static WhitePoint WhitePoint => WhitePoint.From(Illuminant.D65);

            private const double Alpha = 1.09929682680944;
            private const double Beta = 0.018053968510807;

            public static double FromLinear(double linear)
            {
                return Companding.ReflectWhenNegative(linear, e =>
                {
                    if (e < Beta) return 4.5 * e;
                    return Alpha * Math.Pow(e, 0.45) - (Alpha - 1);
                });
            }

            public static double ToLinear(double nonlinear)
            {
                return Companding.ReflectWhenNegative(nonlinear, ePrime =>
                {
                    if (ePrime < Beta * 4.5) return ePrime / 4.5;
                    return Math.Pow((ePrime + (Alpha - 1)) / Alpha, 1 / 0.45);
                });
            }

            public static RgbConfiguration RgbConfiguration => new(R, G, B, WhitePoint, FromLinear, ToLinear);
        }

        public static class A98
        {
            public static readonly Chromaticity R = new(0.6400, 0.3300);
            public static readonly Chromaticity G = new(0.2100, 0.7100);
            public static readonly Chromaticity B = new(0.1500, 0.0600);
            public static WhitePoint WhitePoint => WhitePoint.From(Illuminant.D65);

            public static double FromLinear(double linear)
            {
                return Companding.ReflectWhenNegative(linear, value => Companding.Gamma(value, 563 / 256.0));
            }

            public static double ToLinear(double nonlinear)
            {
                return Companding.ReflectWhenNegative(nonlinear, value => Companding.InverseGamma(value, 563 / 256.0));
            }

            public static RgbConfiguration RgbConfiguration => new(R, G, B, WhitePoint, FromLinear, ToLinear);
        }

        public static class ProPhoto
        {
            public static readonly Chromaticity R = new(0.734699, 0.265301);
            public static readonly Chromaticity G = new(0.159597, 0.840403);
            public static readonly Chromaticity B = new(0.036598, 0.000105);
            public static WhitePoint WhitePoint => WhitePoint.From(Illuminant.D50);

            private const double Et = 1 / 512.0;

            public static double FromLinear(double linear)
            {
                return Companding.ReflectWhenNegative(linear, value =>
                    value < Et
                        ? 16 * value
                        : Companding.Gamma(value, 1.8));
            }

            public static double ToLinear(double nonlinear)
            {
                return Companding.ReflectWhenNegative(nonlinear, value =>
                    value < Et * 16
                        ? value / 16.0
                        : Companding.InverseGamma(value, 1.8));
            }

            public static RgbConfiguration RgbConfiguration => new(R, G, B, WhitePoint, FromLinear, ToLinear);
        }
    }
}