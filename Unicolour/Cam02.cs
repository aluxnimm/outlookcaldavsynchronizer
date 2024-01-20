using System;

namespace Wacton.Unicolour
{

    using static Cam;
    using static Utils;

    public record Cam02 : ColourRepresentation
    {
        protected override int? HueIndex => null;
        public double J => First;
        public double A => Second;
        public double B => Third;
        public Ucs Ucs { get; }
        public Model Model { get; }

        // J lightness bounds not clear (and is different between Model and UCS)
        // presumably also greyscale when A.Equals(0.0) && B.Equals(0.0)
        internal override bool IsGreyscale => Model.Chroma <= 0;

        public Cam02(double j, double a, double b, CamConfiguration camConfig) : this(new Ucs(j, a, b), camConfig,
            ColourHeritage.None)
        {
        }

        internal Cam02(Model model, CamConfiguration camConfig, ColourHeritage heritage) : this(model.ToUcs(),
            camConfig, heritage)
        {
            Model = model;
        }

        internal Cam02(Ucs ucs, CamConfiguration camConfig, ColourHeritage heritage) : base(ucs.J, ucs.A, ucs.B,
            heritage)
        {
            // Model will only be non-null if the constructor that takes Model is called (currently not possible from external code)
            Ucs = ucs;
            Model ??= ucs.ToModel(ViewingConditions(camConfig));
        }

        protected override string FirstString => $"{J:F2}";
        protected override string SecondString => $"{A:+0.00;-0.00;0.00}";
        protected override string ThirdString => $"{B:+0.00;-0.00;0.00}";
        public override string ToString() => base.ToString();

        /*
         * CAM02 is a transform of XYZ
         * Forward: https://doi.org/10.1007/978-1-4419-6190-7_2 · https://doi.org/10.1002/col.20227 · https://doi.org/10.48550/arXiv.1802.06067
         * Reverse: https://doi.org/10.1007/978-1-4419-6190-7_2 · https://doi.org/10.1002/col.20227 · https://doi.org/10.48550/arXiv.1802.06067
         */

        private static readonly Matrix MCAT02 = new(new[,]
        {
            { +0.7328, +0.4296, -0.1624 },
            { -0.7036, +1.6975, +0.0061 },
            { +0.0030, +0.0136, +0.9834 }
        });

        private static readonly Matrix MHPE = new(new[,]
        {
            { +0.38971, +0.68898, -0.07868 },
            { -0.22981, +1.18340, +0.04641 },
            { 0.00000, 0.00000, +1.00000 }
        });

        private static readonly Matrix ForwardStep4 = new(new[,]
        {
            { 2, 1, 1 / 20.0 },
            { 1, -12 / 11.0, 1 / 11.0 },
            { 1 / 9.0, 1 / 9.0, -2 / 9.0 },
            { 1, 1, 21 / 20.0 }
        });

        private static readonly Matrix ReverseStep4 = new Matrix(new double[,]
        {
            { 460, 451, 288 },
            { 460, -891, -261 },
            { 460, -220, -6300 }
        }).Scale(1 / 1403.0);

        private static ViewingConditions ViewingConditions(CamConfiguration camConfig)
        {
            var (xw, yw, zw) = (camConfig.WhitePoint.X, camConfig.WhitePoint.Y, camConfig.WhitePoint.Z);
            var la = camConfig.AdaptingLuminance;
            var yb = camConfig.BackgroundLuminance;
            var c = camConfig.C;
            var f = camConfig.F;
            var nc = camConfig.Nc;

            // step 0
            var xyzWhitePointMatrix = Matrix.FromTriplet(xw, yw, zw);
            var (rw, gw, bw) = MCAT02.Multiply(xyzWhitePointMatrix).ToTriplet();

            var d = (f * (1 - 1 / 3.6 * Math.Exp((-la - 42) / 92.0))).Clamp(0, 1);
            var (dr, dg, db) = (D(rw), D(gw), D(bw));
            double D(double input) => d * (yw / input) + 1 - d;

            var k = 1 / (5 * la + 1);
            var fl = 0.2 * Math.Pow(k, 4) * (5 * la) + 0.1 * Math.Pow(1 - Math.Pow(k, 4), 2) * CubeRoot(5 * la);
            var n = yb / yw;
            var z = 1.48 + Math.Sqrt(n);
            var nbb = 0.725 * Math.Pow(1 / n, 0.2);
            var ncb = nbb;

            // slightly different to CAM16
            var wcMatrix = Matrix.FromTriplet(dr * rw, dg * gw, db * bw);
            var (rwPrime, gwPrime, bwPrime) = MHPE.Multiply(MCAT02.Inverse()).Multiply(wcMatrix).ToTriplet();

            var (raw, gaw, baw) = (Aw(rwPrime), Aw(gwPrime), Aw(bwPrime));

            double Aw(double input)
            {
                var power = Math.Pow(fl * input / 100.0, 0.42);
                return 400 * (power / (power + 27.13)) + 0.1;
            }

            var aw = (2 * raw + gaw + baw / 20.0 - 0.305) * nbb;
            return new ViewingConditions(c, nc, dr, dg, db, fl, n, z, nbb, ncb, aw);
        }

        public static Cam02 FromXyz(Xyz xyz, CamConfiguration camConfig, XyzConfiguration xyzConfig)
        {
            var view = ViewingConditions(camConfig);

            // step 1
            var xyzMatrix = Matrix.FromTriplet(xyz.X, xyz.Y, xyz.Z);
            xyzMatrix = Adaptation.WhitePoint(xyzMatrix, xyzConfig.WhitePoint, camConfig.WhitePoint)
                .Select(x => x * 100);
            var rgb = MCAT02.Multiply(xyzMatrix).ToTriplet();

            // step 2
            var cMatrix = Matrix.FromTriplet(view.Dr * rgb.First, view.Dg * rgb.Second, view.Db * rgb.Third);

            // step 3, slightly different to CAM16
            var (rPrime, gPrime, bPrime) = MHPE.Multiply(MCAT02.Inverse()).Multiply(cMatrix).ToTriplet();

            // step 4
            var (ra, ga, ba) = (A(rPrime), A(gPrime), A(bPrime));

            double A(double input)
            {
                if (double.IsNaN(input)) return double.NaN;
                var power = Math.Pow(view.Fl * Math.Abs(input) / 100.0, 0.42);
                return 400 * Math.Sign(input) * (power / (power + 27.13));
            }

            // step 5
            var aMatrix = Matrix.FromTriplet(ra, ga, ba);
            var components = ForwardStep4.Multiply(aMatrix);
            var (p2, a, b, u) = (components[0, 0], components[1, 0], components[2, 0], components[3, 0]);
            var h = ToDegrees(Math.Atan2(b, a)).Modulo(360);

            // step 6
            var et = HueData.GetEccentricity(h);

            // step 7
            var achromatic = p2 * view.Nbb;

            // step 8
            var j = 100 * Math.Pow(achromatic / view.Aw, view.C * view.Z);

            // step 9
            var q = 4 / view.C * Math.Pow(j / 100.0, 0.5) * (view.Aw + 4) * Math.Pow(view.Fl, 0.25);

            // step 10
            var t = 50000 / 13.0 * view.Nc * view.Ncb * et * Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2)) / (u + 0.305);
            var alpha = Math.Pow(t, 0.9) * Math.Pow(1.64 - Math.Pow(0.29, view.N), 0.73);
            var c = alpha * Math.Sqrt(j / 100.0);
            var m = c * Math.Pow(view.Fl, 0.25);
            var s = 50 * Math.Sqrt(alpha * view.C / (view.Aw + 4));
            return new Cam02(new Model(j, c, h, m, s, q), camConfig, ColourHeritage.From(xyz));
        }

        public static Xyz ToXyz(Cam02 cam, CamConfiguration camConfig, XyzConfiguration xyzConfig)
        {
            var view = ViewingConditions(camConfig);

            var j = cam.Model.J;
            var c = cam.Model.C;
            var h = cam.Model.H;

            // step 1
            var alpha = j == 0 ? 0 : c / Math.Sqrt(j / 100.0);
            var t = Math.Pow(alpha / Math.Pow(1.64 - Math.Pow(0.29, view.N), 0.73), 1 / 0.9);

            // step 2
            var et = 0.25 * (Math.Cos(ToRadians(h) + 2) + 3.8);
            var achromatic = view.Aw * Math.Pow(j / 100.0, 1 / (view.C * view.Z));
            var p1 = et * (50000 / 13.0) * view.Nc * view.Ncb;
            var p2 = achromatic / view.Nbb;

            // step 3
            var gamma = 23 * (p2 + 0.305) * t /
                        (23 * p1 + 11 * t * Math.Cos(ToRadians(h)) + 108 * t * Math.Sin(ToRadians(h)));
            var a = gamma * Math.Cos(ToRadians(h));
            var b = gamma * Math.Sin(ToRadians(h));

            // step 4
            var components = Matrix.FromTriplet(p2, a, b);
            var (ra, ga, ba) = ReverseStep4.Multiply(components).ToTriplet();

            // step 5, slightly different to CAM16
            var primeMatrix = Matrix.FromTriplet(C(ra), C(ga), C(ba));

            double C(double input)
            {
                if (double.IsNaN(input)) return double.NaN;
                return Math.Sign(input) * (100 / view.Fl) *
                       Math.Pow(27.13 * Math.Abs(input) / (400 - Math.Abs(input)), 1 / 0.42);
            }

            var (rc, gc, bc) = MCAT02.Multiply(MHPE.Inverse()).Multiply(primeMatrix).ToTriplet();

            // step 6
            var rgbMatrix = Matrix.FromTriplet(rc / view.Dr, gc / view.Dg, bc / view.Db);

            // step 7
            var xyzMatrix = MCAT02.Inverse().Multiply(rgbMatrix);
            xyzMatrix = Adaptation.WhitePoint(xyzMatrix, camConfig.WhitePoint, xyzConfig.WhitePoint)
                .Select(x => x / 100.0);
            return new Xyz(xyzMatrix.ToTriplet(), ColourHeritage.From(cam));
        }
    }
}

