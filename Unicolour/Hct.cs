using System;
using System.Collections.Generic;

namespace Wacton.Unicolour
{

    public record Hct : ColourRepresentation
    {
        protected override int? HueIndex => 0;
        public double H => First;
        public double C => Second;
        public double T => Third;
        public double ConstrainedH => ConstrainedFirst;
        protected override double ConstrainedFirst => H.Modulo(360.0);
        internal override bool IsGreyscale => C <= 0.0 || T is <= 0.0 or >= 100.0;

        public Hct(double h, double c, double t) : this(h, c, t, ColourHeritage.None)
        {
        }

        internal Hct(double h, double c, double t, ColourHeritage heritage) : base(h, c, t, heritage)
        {
        }

        protected override string FirstString => UseAsHued ? $"{H:F1}°" : "—°";
        protected override string SecondString => $"{C:F2}";
        protected override string ThirdString => $"{T:F2}";
        public override string ToString() => base.ToString();

        /*
         * HCT is a transform of XYZ
         * (just a combination of LAB & CAM16, but with specific XYZ & CAM configuration, so can't reuse existing colour space calculations)
         * Forward: https://material.io/blog/science-of-color-design
         * Reverse: n/a - no published reverse transform and I don't want to port Google code, so using my own naive search
         */

        internal static Cam16 Cam16Component(Xyz xyz) => Cam16.FromXyz(xyz, CamConfiguration.Hct, XyzConfiguration.D65);
        internal static Lab LabComponent(Xyz xyz) => Lab.FromXyz(xyz, XyzConfiguration.D65);

        internal static Hct FromXyz(Xyz xyz, XyzConfiguration xyzConfig)
        {
            var xyzMatrix = Matrix.FromTriplet(xyz.Triplet);
            var d65Matrix = Adaptation.WhitePoint(xyzMatrix, xyzConfig.WhitePoint, WhitePoint.From(Illuminant.D65));
            var d65Xyz = new Xyz(d65Matrix.ToTriplet(), ColourHeritage.From(xyz));

            var cam16 = Cam16Component(d65Xyz);
            var lab = LabComponent(d65Xyz);

            var h = cam16.Model.H;
            var c = cam16.Model.C;
            var t = lab.L;
            return new Hct(h, c, t, ColourHeritage.From(xyz));
        }

        internal static Xyz ToXyz(Hct hct, XyzConfiguration xyzConfig)
        {
            var targetY = Lab.ToXyz(new Lab(hct.T, 0, 0), XyzConfiguration.D65).Y;
            var result = FindBestJ(targetY, hct);
            var d65Xyz = result.Converged ? result.Data.Xyz : new Xyz(double.NaN, double.NaN, double.NaN);
            var d65Matrix = Matrix.FromTriplet(d65Xyz.Triplet);
            var xyzMatrix = Adaptation.WhitePoint(d65Matrix, WhitePoint.From(Illuminant.D65), xyzConfig.WhitePoint);
            var xyz = new Xyz(xyzMatrix.ToTriplet(), ColourHeritage.From(hct))
            {
                HctToXyzSearchResult = result
            };

            return xyz;
        }

        // i'm sure some smart people have some fancy-pants algorithms to do this efficiently
        // but until there's some kind of published reverse transformation algorithm, this gets the job done
        // (albeit rather slowly...)
        private static HctToXyzSearchResult FindBestJ(double targetY, Hct hct)
        {
            HctToXyzSearchData latest = GetStartingData(targetY, hct);
            HctToXyzSearchData best = latest;

            var step = latest.J;
            var iterations = 0;
            while (!double.IsNaN(latest.DeltaY) && Math.Abs(latest.DeltaY) > 0.000000001 && iterations < 100)
            {
                var j = latest.J + (latest.DeltaY > 0 ? -step : step);
                var data = ProcessJ(targetY, j, hct);
                var deltaY = data.DeltaY;
                if (Math.Abs(deltaY) < Math.Abs(best.DeltaY))
                {
                    best = data;
                }

                // change in sign of delta means target is now in the other direction
                var overshot = double.IsNaN(deltaY) || Math.Sign(latest.DeltaY) != Math.Sign(deltaY);
                if (overshot)
                {
                    step /= 2.0;
                }

                latest = data;
                iterations++;
            }

            var converged = !double.IsNaN(latest.DeltaY) && iterations < 100;
            return new HctToXyzSearchResult(best, iterations, converged);
        }

        private static HctToXyzSearchData GetStartingData(double targetY, Hct hct)
        {
            var xzPairs = new List<(double, double)> { (0, 0), (0, 1), (1, 0), (1, 1) };
            HctToXyzSearchData best = InitialData;
            foreach (var (x, z) in xzPairs)
            {
                var j = Cam16.FromXyz(new Xyz(x, targetY, z), CamConfiguration.Hct, XyzConfiguration.D65).Model.J;
                var data = ProcessJ(targetY, j, hct);
                if (Math.Abs(data.DeltaY) < best.DeltaY)
                {
                    best = data;
                }
            }

            return best;
        }

        private static HctToXyzSearchData ProcessJ(double targetY, double j, Hct hct)
        {
            var camModel = new Cam.Model(j, hct.C, hct.H, 0, 0, 0);
            var cam16 = new Cam16(camModel, CamConfiguration.Hct, ColourHeritage.None);
            var xyz = Cam16.ToXyz(cam16, CamConfiguration.Hct, XyzConfiguration.D65);
            var deltaY = xyz.Y - targetY;
            return new HctToXyzSearchData(hct, j, cam16, xyz, targetY, deltaY);
        }

        private static readonly HctToXyzSearchData InitialData = new(
            J: double.PositiveInfinity, DeltaY: double.PositiveInfinity,
            Hct: null!, Cam16: null!, Xyz: null!, TargetY: double.NaN);
    }

// only for potential debugging or diagnostics
// until there is an "official" HCT -> XYZ reverse transform
    internal record HctToXyzSearchResult(HctToXyzSearchData Data, int Iterations, bool Converged)
    {
        internal HctToXyzSearchData Data { get; } = Data;
        internal int Iterations { get; } = Iterations;
        internal bool Converged { get; } = Converged;
        public override string ToString() => $"{Data} · Iterations:{Iterations} · Converged:{Converged}";
    }

    internal record HctToXyzSearchData(Hct Hct, double J, Cam16 Cam16, Xyz Xyz, double TargetY, double DeltaY)
    {
        internal Hct Hct { get; } = Hct;
        internal double J { get; } = J;
        internal Cam16 Cam16 { get; } = Cam16;
        internal Xyz Xyz { get; } = Xyz;
        internal double TargetY { get; } = TargetY;
        internal double DeltaY { get; } = DeltaY;
        public override string ToString() => $"J:{J:F4} · ΔY:{DeltaY:F4}";
    }
}