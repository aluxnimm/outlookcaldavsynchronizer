namespace Wacton.Unicolour
{

    internal static class GamutMapping
    {
        /*
         * adapted from https://www.w3.org/TR/css-color-4/#css-gamut-mapping & https://www.w3.org/TR/css-color-4/#binsearch
         * the pseudocode doesn't appear to handle the edge case scenario where:
         * a) origin colour OKLCH chroma < epsilon
         * b) origin colour destination (RGB here) is out-of-gamut
         * e.g. OKLCH (0.99999, 0, 0) --> RGB (1.00010, 0.99998, 0.99974)
         * - the search never executes since chroma = 0 (min 0 - max 0 < epsilon 0.0001)
         * - even if the search did execute, would not return clipped variant since ΔE is *too small*, and min never changes from 0
         * so need to clip if the mapped colour is somehow out-of-gamut (i.e. not processed)
         */
        internal static Unicolour ToRgbGamut(Unicolour unicolour)
        {
            var config = unicolour.Config;
            var rgb = unicolour.Rgb;
            var alpha = unicolour.Alpha.A;
            if (unicolour.IsInDisplayGamut) return new Unicolour(ColourSpace.Rgb, config, rgb.Triplet.Tuple, alpha);

            var oklch = unicolour.Oklch;
            if (oklch.L >= 1.0) return new Unicolour(ColourSpace.Rgb, config, 1, 1, 1, alpha);
            if (oklch.L <= 0.0) return new Unicolour(ColourSpace.Rgb, config, 0, 0, 0, alpha);

            const double jnd = 0.02;
            const double epsilon = 0.0001;
            var minChroma = 0.0;
            var maxChroma = oklch.C;
            var minChromaInGamut = true;

            // iteration count ensures the while loop doesn't get stuck in an endless cycle if bad input is provided
            // e.g. double.Epsilon
            var iterations = 0;
            Unicolour? current = null;
            bool HasChromaConverged() => maxChroma - minChroma <= epsilon;
            while (!HasChromaConverged() && iterations < 1000)
            {
                iterations++;

                var chroma = (minChroma + maxChroma) / 2.0;
                current = FromOklchWithChroma(chroma);

                if (minChromaInGamut && current.Rgb.IsInGamut)
                {
                    minChroma = chroma;
                    continue;
                }

                var clipped = FromRgbWithClipping(current.Rgb);
                var deltaE = clipped.Difference(DeltaE.Ok, current);

                var isNoticeableDifference = deltaE >= jnd;
                if (isNoticeableDifference)
                {
                    maxChroma = chroma;
                }
                else
                {
                    // not clear to me why a clipped colour must have ΔE from "current" colour between 0.0199 - 0.02
                    // effectively: only returning clipped when ΔE == JND, but continue if the non-noticeable ΔE is *too small*
                    // but I assume it's something to do with this comment about intersecting shallow and concave gamut boundaries
                    // https://github.com/w3c/csswg-drafts/issues/7653#issuecomment-1489096489
                    var isUnnoticeableDifferenceLargeEnough = jnd - deltaE < epsilon;
                    if (isUnnoticeableDifferenceLargeEnough)
                    {
                        return clipped;
                    }

                    minChromaInGamut = false;
                    minChroma = chroma;
                }
            }

            // in case while loop never executes (e.g. Oklch.C == 0)
            current ??= FromOklchWithChroma(oklch.C);

            // it's possible for the "current" colour to still be out of RGB gamut, either because:
            // a) the original OKLCH was not processed (chroma too low) and was already out of RGB gamut
            // b) the algorithm converged on an OKLCH that is out of RGB gamut (happens ~5% of the time for me with using random OKLCH inputs)
            return current.IsInDisplayGamut ? current : FromRgbWithClipping(current.Rgb);

            Unicolour FromOklchWithChroma(double chroma) =>
                new(ColourSpace.Oklch, config, oklch.L, chroma, oklch.H, alpha);

            Unicolour FromRgbWithClipping(Rgb unclippedRgb) =>
                new(ColourSpace.Rgb, config, unclippedRgb.ConstrainedTriplet.Tuple, alpha);
        }
    }
}