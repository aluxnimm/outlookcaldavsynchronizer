namespace Wacton.Unicolour
{

    public class XyzConfiguration
    {
        public WhitePoint WhitePoint { get; }
        public Chromaticity ChromaticityWhite { get; }

        public static readonly XyzConfiguration D65 = new(WhitePoint.From(Illuminant.D65));
        public static readonly XyzConfiguration D50 = new(WhitePoint.From(Illuminant.D50));

        public XyzConfiguration(WhitePoint whitePoint)
        {
            WhitePoint = whitePoint;

            var x = WhitePoint.X / 100.0;
            var y = WhitePoint.Y / 100.0;
            var z = WhitePoint.Z / 100.0;
            var normalisation = x + y + z;
            ChromaticityWhite = new(x / normalisation, y / normalisation);
        }

        public override string ToString() => $"XYZ {WhitePoint}";
    }
}