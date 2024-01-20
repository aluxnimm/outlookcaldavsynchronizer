namespace Wacton.Unicolour
{

    public record Chromaticity(double X, double Y)
    {
        public double X { get; } = X;
        public double Y { get; } = Y;
        public override string ToString() => $"({X:F4}, {Y:F4})";
    }
}