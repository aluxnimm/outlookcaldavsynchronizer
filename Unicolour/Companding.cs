using System;

namespace Wacton.Unicolour
{

    public static class Companding
    {
        public static double Gamma(double value, double gamma) => Math.Pow(value, 1 / gamma);
        public static double InverseGamma(double value, double gamma) => Math.Pow(value, gamma);

        public static double ReflectWhenNegative(double value, Func<double, double> function)
        {
            if (double.IsNaN(value)) return double.NaN;
            return Math.Sign(value) * function(Math.Abs(value));
        }
    }
}