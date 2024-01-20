using System;

namespace Wacton.Unicolour
{

    public class RgbConfiguration
    {
        public static readonly RgbConfiguration StandardRgb = RgbModels.StandardRgb.RgbConfiguration;
        public static readonly RgbConfiguration DisplayP3 = RgbModels.DisplayP3.RgbConfiguration;
        public static readonly RgbConfiguration Rec2020 = RgbModels.Rec2020.RgbConfiguration;
        public static readonly RgbConfiguration A98 = RgbModels.A98.RgbConfiguration;
        public static readonly RgbConfiguration ProPhoto = RgbModels.ProPhoto.RgbConfiguration;

        public Chromaticity ChromaticityR { get; }
        public Chromaticity ChromaticityG { get; }
        public Chromaticity ChromaticityB { get; }
        public WhitePoint WhitePoint { get; }
        public Func<double, double> CompandFromLinear { get; }
        public Func<double, double> InverseCompandToLinear { get; }

        public RgbConfiguration(
            Chromaticity chromaticityR,
            Chromaticity chromaticityG,
            Chromaticity chromaticityB,
            WhitePoint whitePoint,
            Func<double, double> fromLinear,
            Func<double, double> toLinear)
        {
            ChromaticityR = chromaticityR;
            ChromaticityG = chromaticityG;
            ChromaticityB = chromaticityB;
            WhitePoint = whitePoint;
            CompandFromLinear = fromLinear;
            InverseCompandToLinear = toLinear;
        }

        public override string ToString() => $"RGB {WhitePoint} {ChromaticityR} {ChromaticityG} {ChromaticityB}";
    }
}