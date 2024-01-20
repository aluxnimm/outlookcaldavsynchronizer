using System;

namespace Wacton.Unicolour
{

    public class Configuration
    {
        internal readonly Guid Id = Guid.NewGuid();

        public RgbConfiguration Rgb { get; }
        public XyzConfiguration Xyz { get; }
        public CamConfiguration Cam { get; }
        public double IctcpScalar { get; }
        public double JzazbzScalar { get; }

        public static readonly Configuration Default = new();

        public Configuration(
            RgbConfiguration? rgbConfiguration = null,
            XyzConfiguration? xyzConfiguration = null,
            CamConfiguration? camConfiguration = null,
            double ictcpScalar = 100,
            double jzazbzScalar = 100)
        {
            Rgb = rgbConfiguration ?? RgbConfiguration.StandardRgb;
            Xyz = xyzConfiguration ?? XyzConfiguration.D65;
            Cam = camConfiguration ?? CamConfiguration.StandardRgb;
            IctcpScalar = ictcpScalar;
            JzazbzScalar = jzazbzScalar;
        }

        public override string ToString() => $"{Id}";
    }
}