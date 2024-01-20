using System;

namespace Wacton.Unicolour
{

    public record Alpha(double A)
    {
        public double A { get; } = A;
        public double ConstrainedA => double.IsNaN(A) ? 0 : A.Clamp(0.0, 1.0);
        public int A255 => (int)Math.Round(ConstrainedA * 255);
        public string Hex => $"{A255:X2}";

        public override string ToString() => $"{A:F2}";
    }
}