using System;

namespace Wacton.Unicolour
{
/*
 * ColourSpace information needs to be held outwith ColourRepresentation object
 * otherwise the ColourRepresentation needs to be evaluated just to obtain the ColourSpace it represents
 */
    public partial class Unicolour
    {
        private static ColourRepresentation CreateRepresentation(
            ColourSpace colourSpace, double first, double second, double third,
            Configuration config, ColourHeritage heritage)
        {
            return colourSpace switch
            {
                ColourSpace.Rgb => new Rgb(first, second, third, heritage),
                ColourSpace.RgbLinear => new RgbLinear(first, second, third, heritage),
                ColourSpace.Hsb => new Hsb(first, second, third, heritage),
                ColourSpace.Hsl => new Hsl(first, second, third, heritage),
                ColourSpace.Hwb => new Hwb(first, second, third, heritage),
                ColourSpace.Xyz => new Xyz(first, second, third, heritage),
                ColourSpace.Xyy => new Xyy(first, second, third, heritage),
                ColourSpace.Lab => new Lab(first, second, third, heritage),
                ColourSpace.Lchab => new Lchab(first, second, third, heritage),
                ColourSpace.Luv => new Luv(first, second, third, heritage),
                ColourSpace.Lchuv => new Lchuv(first, second, third, heritage),
                ColourSpace.Hsluv => new Hsluv(first, second, third, heritage),
                ColourSpace.Hpluv => new Hpluv(first, second, third, heritage),
                ColourSpace.Ictcp => new Ictcp(first, second, third, heritage),
                ColourSpace.Jzazbz => new Jzazbz(first, second, third, heritage),
                ColourSpace.Jzczhz => new Jzczhz(first, second, third, heritage),
                ColourSpace.Oklab => new Oklab(first, second, third, heritage),
                ColourSpace.Oklch => new Oklch(first, second, third, heritage),
                ColourSpace.Cam02 => new Cam02(new Cam.Ucs(first, second, third), config.Cam, heritage),
                ColourSpace.Cam16 => new Cam16(new Cam.Ucs(first, second, third), config.Cam, heritage),
                ColourSpace.Hct => new Hct(first, second, third, heritage),
                _ => throw new ArgumentOutOfRangeException(nameof(colourSpace), colourSpace, null)
            };
        }

        public ColourRepresentation GetRepresentation(ColourSpace colourSpace)
        {
            return colourSpace switch
            {
                ColourSpace.Rgb => Rgb,
                ColourSpace.Rgb255 => Rgb.Byte255,
                ColourSpace.RgbLinear => RgbLinear,
                ColourSpace.Hsb => Hsb,
                ColourSpace.Hsl => Hsl,
                ColourSpace.Hwb => Hwb,
                ColourSpace.Xyz => Xyz,
                ColourSpace.Xyy => Xyy,
                ColourSpace.Lab => Lab,
                ColourSpace.Lchab => Lchab,
                ColourSpace.Luv => Luv,
                ColourSpace.Lchuv => Lchuv,
                ColourSpace.Hsluv => Hsluv,
                ColourSpace.Hpluv => Hpluv,
                ColourSpace.Ictcp => Ictcp,
                ColourSpace.Jzazbz => Jzazbz,
                ColourSpace.Jzczhz => Jzczhz,
                ColourSpace.Oklab => Oklab,
                ColourSpace.Oklch => Oklch,
                ColourSpace.Cam02 => Cam02,
                ColourSpace.Cam16 => Cam16,
                ColourSpace.Hct => Hct,
                _ => throw new ArgumentOutOfRangeException(nameof(colourSpace), colourSpace, null)
            };
        }

        /*
         * getting a value will trigger a chain of gets and conversions if the intermediary values have not been calculated yet
         * e.g. if Unicolour is created from RGB, and the first request is for LAB:
         * - Get(ColourSpace.Lab); lab is null, execute: lab = Lab.FromXyz(Xyz, Config)
         * - Get(ColourSpace.Xyz); xyz is null, execute: xyz = Rgb.ToXyz(Rgb, Config)
         * - Get(ColourSpace.Rgb); rgb is not null, return value
         * - xyz is evaluated from rgb and stored
         * - lab is evaluated from xyz and stored
         */
        private T Get<T>(ColourSpace targetSpace) where T : ColourRepresentation
        {
            var backingRepresentation = GetBackingField(targetSpace);
            if (backingRepresentation == null)
            {
                SetBackingField(targetSpace);
                backingRepresentation = GetBackingField(targetSpace);
            }

            return (backingRepresentation as T)!;
        }

        private ColourRepresentation? GetBackingField(ColourSpace colourSpace)
        {
            return colourSpace switch
            {
                ColourSpace.Rgb => rgb,
                ColourSpace.RgbLinear => rgbLinear,
                ColourSpace.Hsb => hsb,
                ColourSpace.Hsl => hsl,
                ColourSpace.Hwb => hwb,
                ColourSpace.Xyz => xyz,
                ColourSpace.Xyy => xyy,
                ColourSpace.Lab => lab,
                ColourSpace.Lchab => lchab,
                ColourSpace.Luv => luv,
                ColourSpace.Lchuv => lchuv,
                ColourSpace.Hsluv => hsluv,
                ColourSpace.Hpluv => hpluv,
                ColourSpace.Ictcp => ictcp,
                ColourSpace.Jzazbz => jzazbz,
                ColourSpace.Jzczhz => jzczhz,
                ColourSpace.Oklab => oklab,
                ColourSpace.Oklch => oklch,
                ColourSpace.Cam02 => cam02,
                ColourSpace.Cam16 => cam16,
                ColourSpace.Hct => hct,
                _ => throw new ArgumentOutOfRangeException(nameof(colourSpace), colourSpace, null)
            };
        }

        private void SetBackingField(ColourSpace targetSpace)
        {
            Action setField = targetSpace switch
            {
                ColourSpace.Rgb => () => rgb = EvaluateRgb(),
                ColourSpace.RgbLinear => () => rgbLinear = EvaluateRgbLinear(),
                ColourSpace.Hsb => () => hsb = EvaluateHsb(),
                ColourSpace.Hsl => () => hsl = EvaluateHsl(),
                ColourSpace.Hwb => () => hwb = EvaluateHwb(),
                ColourSpace.Xyz => () => xyz = EvaluateXyz(),
                ColourSpace.Xyy => () => xyy = EvaluateXyy(),
                ColourSpace.Lab => () => lab = EvaluateLab(),
                ColourSpace.Lchab => () => lchab = EvaluateLchab(),
                ColourSpace.Luv => () => luv = EvaluateLuv(),
                ColourSpace.Lchuv => () => lchuv = EvaluateLchuv(),
                ColourSpace.Hsluv => () => hsluv = EvaluateHsluv(),
                ColourSpace.Hpluv => () => hpluv = EvaluateHpluv(),
                ColourSpace.Ictcp => () => ictcp = EvaluateIctcp(),
                ColourSpace.Jzazbz => () => jzazbz = EvaluateJzazbz(),
                ColourSpace.Jzczhz => () => jzczhz = EvaluateJzczhz(),
                ColourSpace.Oklab => () => oklab = EvaluateOklab(),
                ColourSpace.Oklch => () => oklch = EvaluateOklch(),
                ColourSpace.Cam02 => () => cam02 = EvaluateCam02(),
                ColourSpace.Cam16 => () => cam16 = EvaluateCam16(),
                ColourSpace.Hct => () => hct = EvaluateHct(),
                _ => throw new ArgumentOutOfRangeException(nameof(targetSpace), targetSpace, null)
            };

            setField();
        }

        /*
         * evaluation method switch expressions are arranged as follows:
         * - first item     = target space is the initial space, simply return initial representation
         * - middle items   = reverse transforms to the target space; only the immediate transforms
         * - default item   = forward transform from a base space
         * -----------------
         * only need to consider the transforms relative to the target-space, as subsequent transforms are handled recursively
         * e.g. for target-space RGB...
         * - starting at HSL:
         *   - transforms: HSL ==reverse==> HSB ==reverse==> RGB
         *   - only need to specify: HSB ==reverse==> RGB
         *   - function: Hsb.ToRgb()
         * - starting at LAB:
         *   - LAB ==reverse==> XYZ ==forward==> RGB Linear ==forward==> RGB
         *   - only need to specify: RGB Linear ==forward==> RGB
         *   - function: Rgb.FromRgbLinear()
         */

        private Rgb EvaluateRgb()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Rgb => (Rgb)InitialRepresentation,
                ColourSpace.Hsb => Hsb.ToRgb(Hsb),
                ColourSpace.Hsl => Hsb.ToRgb(Hsb),
                ColourSpace.Hwb => Hsb.ToRgb(Hsb),
                _ => Rgb.FromRgbLinear(RgbLinear, Config.Rgb)
            };
        }

        private RgbLinear EvaluateRgbLinear()
        {
            return InitialColourSpace switch
            {
                ColourSpace.RgbLinear => (RgbLinear)InitialRepresentation,
                ColourSpace.Rgb => Rgb.ToRgbLinear(Rgb, Config.Rgb),
                ColourSpace.Hsb => Rgb.ToRgbLinear(Rgb, Config.Rgb),
                ColourSpace.Hsl => Rgb.ToRgbLinear(Rgb, Config.Rgb),
                ColourSpace.Hwb => Rgb.ToRgbLinear(Rgb, Config.Rgb),
                _ => RgbLinear.FromXyz(Xyz, Config.Rgb, Config.Xyz)
            };
        }

        private Hsb EvaluateHsb()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Hsb => (Hsb)InitialRepresentation,
                ColourSpace.Hsl => Hsl.ToHsb(Hsl),
                ColourSpace.Hwb => Hwb.ToHsb(Hwb),
                _ => Hsb.FromRgb(Rgb)
            };
        }

        private Hsl EvaluateHsl()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Hsl => (Hsl)InitialRepresentation,
                _ => Hsl.FromHsb(Hsb)
            };
        }

        private Hwb EvaluateHwb()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Hwb => (Hwb)InitialRepresentation,
                _ => Hwb.FromHsb(Hsb)
            };
        }

        private Xyz EvaluateXyz()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Xyz => (Xyz)InitialRepresentation,
                ColourSpace.Rgb => RgbLinear.ToXyz(RgbLinear, Config.Rgb, Config.Xyz),
                ColourSpace.RgbLinear => RgbLinear.ToXyz(RgbLinear, Config.Rgb, Config.Xyz),
                ColourSpace.Hsb => RgbLinear.ToXyz(RgbLinear, Config.Rgb, Config.Xyz),
                ColourSpace.Hsl => RgbLinear.ToXyz(RgbLinear, Config.Rgb, Config.Xyz),
                ColourSpace.Hwb => RgbLinear.ToXyz(RgbLinear, Config.Rgb, Config.Xyz),
                ColourSpace.Xyy => Xyy.ToXyz(Xyy),
                ColourSpace.Lab => Lab.ToXyz(Lab, Config.Xyz),
                ColourSpace.Lchab => Lab.ToXyz(Lab, Config.Xyz),
                ColourSpace.Luv => Luv.ToXyz(Luv, Config.Xyz),
                ColourSpace.Lchuv => Luv.ToXyz(Luv, Config.Xyz),
                ColourSpace.Hsluv => Luv.ToXyz(Luv, Config.Xyz),
                ColourSpace.Hpluv => Luv.ToXyz(Luv, Config.Xyz),
                ColourSpace.Ictcp => Ictcp.ToXyz(Ictcp, Config.IctcpScalar, Config.Xyz),
                ColourSpace.Jzazbz => Jzazbz.ToXyz(Jzazbz, Config.JzazbzScalar, Config.Xyz),
                ColourSpace.Jzczhz => Jzazbz.ToXyz(Jzazbz, Config.JzazbzScalar, Config.Xyz),
                ColourSpace.Oklab => Oklab.ToXyz(Oklab, Config.Xyz),
                ColourSpace.Oklch => Oklab.ToXyz(Oklab, Config.Xyz),
                ColourSpace.Cam02 => Cam02.ToXyz(Cam02, Config.Cam, Config.Xyz),
                ColourSpace.Cam16 => Cam16.ToXyz(Cam16, Config.Cam, Config.Xyz),
                ColourSpace.Hct => Hct.ToXyz(Hct, Config.Xyz),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private Xyy EvaluateXyy()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Xyy => (Xyy)InitialRepresentation,
                _ => Xyy.FromXyz(Xyz, Config.Xyz)
            };
        }

        private Lab EvaluateLab()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Lab => (Lab)InitialRepresentation,
                ColourSpace.Lchab => Lchab.ToLab(Lchab),
                _ => Lab.FromXyz(Xyz, Config.Xyz)
            };
        }

        private Lchab EvaluateLchab()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Lchab => (Lchab)InitialRepresentation,
                _ => Lchab.FromLab(Lab)
            };
        }

        private Luv EvaluateLuv()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Luv => (Luv)InitialRepresentation,
                ColourSpace.Lchuv => Lchuv.ToLuv(Lchuv),
                ColourSpace.Hsluv => Lchuv.ToLuv(Lchuv),
                ColourSpace.Hpluv => Lchuv.ToLuv(Lchuv),
                _ => Luv.FromXyz(Xyz, Config.Xyz)
            };
        }

        private Lchuv EvaluateLchuv()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Lchuv => (Lchuv)InitialRepresentation,
                ColourSpace.Hsluv => Hsluv.ToLchuv(Hsluv),
                ColourSpace.Hpluv => Hpluv.ToLchuv(Hpluv),
                _ => Lchuv.FromLuv(Luv)
            };
        }

        private Hsluv EvaluateHsluv()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Hsluv => (Hsluv)InitialRepresentation,
                _ => Hsluv.FromLchuv(Lchuv)
            };
        }

        private Hpluv EvaluateHpluv()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Hpluv => (Hpluv)InitialRepresentation,
                _ => Hpluv.FromLchuv(Lchuv)
            };
        }

        private Ictcp EvaluateIctcp()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Ictcp => (Ictcp)InitialRepresentation,
                _ => Ictcp.FromXyz(Xyz, Config.IctcpScalar, Config.Xyz)
            };
        }

        private Jzazbz EvaluateJzazbz()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Jzazbz => (Jzazbz)InitialRepresentation,
                ColourSpace.Jzczhz => Jzczhz.ToJzazbz(Jzczhz),
                _ => Jzazbz.FromXyz(Xyz, Config.JzazbzScalar, Config.Xyz)
            };
        }

        private Jzczhz EvaluateJzczhz()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Jzczhz => (Jzczhz)InitialRepresentation,
                _ => Jzczhz.FromJzazbz(Jzazbz)
            };
        }

        private Oklab EvaluateOklab()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Oklab => (Oklab)InitialRepresentation,
                ColourSpace.Oklch => Oklch.ToOklab(Oklch),
                _ => Oklab.FromXyz(Xyz, Config.Xyz)
            };
        }

        private Oklch EvaluateOklch()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Oklch => (Oklch)InitialRepresentation,
                _ => Oklch.FromOklab(Oklab)
            };
        }

        private Cam02 EvaluateCam02()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Cam02 => (Cam02)InitialRepresentation,
                _ => Cam02.FromXyz(Xyz, Config.Cam, Config.Xyz)
            };
        }

        private Cam16 EvaluateCam16()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Cam16 => (Cam16)InitialRepresentation,
                _ => Cam16.FromXyz(Xyz, Config.Cam, Config.Xyz)
            };
        }

        private Hct EvaluateHct()
        {
            return InitialColourSpace switch
            {
                ColourSpace.Hct => (Hct)InitialRepresentation,
                _ => Hct.FromXyz(Xyz, Config.Xyz)
            };
        }
    }
}