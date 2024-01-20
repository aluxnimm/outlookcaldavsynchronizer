using System;

namespace Wacton.Unicolour
{

    public record ColourTriplet(double First, double Second, double Third, int? HueIndex = null)
    {
        public double First { get; } = First;
        public double Second { get; } = Second;
        public double Third { get; } = Third;
        public (double, double, double) Tuple => (First, Second, Third);
        public int? HueIndex { get; } = HueIndex;

        public double[] AsArray() => new[] { First, Second, Third };

        internal double HueValue()
        {
            return HueIndex switch
            {
                0 => First,
                2 => Third,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        internal ColourTriplet WithHueOverride(double hue)
        {
            return HueIndex switch
            {
                0 => new(hue, Second, Third, HueIndex),
                2 => new(First, Second, hue, HueIndex),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        internal ColourTriplet WithHueModulo()
        {
            return HueIndex switch
            {
                0 => new(First.Modulo(360), Second, Third, HueIndex),
                2 => new(First, Second, Third.Modulo(360), HueIndex),
                null => new(First, Second, Third, HueIndex),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        internal ColourTriplet WithPremultipliedAlpha(double alpha)
        {
            return HueIndex switch
            {
                0 => new(First, Second * alpha, Third * alpha, HueIndex),
                2 => new(First * alpha, Second * alpha, Third, HueIndex),
                null => new(First * alpha, Second * alpha, Third * alpha, HueIndex),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        internal ColourTriplet WithUnpremultipliedAlpha(double alpha)
        {
            if (alpha == 0)
            {
                alpha = 1.0;
            }

            return HueIndex switch
            {
                0 => new(First, Second / alpha, Third / alpha, HueIndex),
                2 => new(First / alpha, Second / alpha, Third, HueIndex),
                null => new(First / alpha, Second / alpha, Third / alpha, HueIndex),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        // need a custom deconstruct to ignore the nullable hue index
        public void Deconstruct(out double first, out double second, out double third)
        {
            first = First;
            second = Second;
            third = Third;
        }

        public override string ToString() => Tuple.ToString();
    }
}