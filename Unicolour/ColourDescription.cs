using System.Collections.Generic;

namespace Wacton.Unicolour
{

    internal record ColourDescription(string description)
    {
        private readonly string description = description;

        internal static readonly ColourDescription NotApplicable = new("-");
        internal static readonly ColourDescription Black = new(nameof(Black));
        internal static readonly ColourDescription Shadow = new(nameof(Shadow));
        internal static readonly ColourDescription Dark = new(nameof(Dark));
        internal static readonly ColourDescription Pure = new(nameof(Pure));
        internal static readonly ColourDescription Light = new(nameof(Light));
        internal static readonly ColourDescription Pale = new(nameof(Pale));
        internal static readonly ColourDescription White = new(nameof(White));
        internal static readonly ColourDescription Grey = new(nameof(Grey));
        internal static readonly ColourDescription Faint = new(nameof(Faint));
        internal static readonly ColourDescription Weak = new(nameof(Weak));
        internal static readonly ColourDescription Mild = new(nameof(Mild));
        internal static readonly ColourDescription Strong = new(nameof(Strong));
        internal static readonly ColourDescription Vibrant = new(nameof(Vibrant));
        internal static readonly ColourDescription Red = new(nameof(Red));
        internal static readonly ColourDescription Orange = new(nameof(Orange));
        internal static readonly ColourDescription Yellow = new(nameof(Yellow));
        internal static readonly ColourDescription Chartreuse = new(nameof(Chartreuse));
        internal static readonly ColourDescription Green = new(nameof(Green));
        internal static readonly ColourDescription Mint = new(nameof(Mint));
        internal static readonly ColourDescription Cyan = new(nameof(Cyan));
        internal static readonly ColourDescription Azure = new(nameof(Azure));
        internal static readonly ColourDescription Blue = new(nameof(Blue));
        internal static readonly ColourDescription Violet = new(nameof(Violet));
        internal static readonly ColourDescription Magenta = new(nameof(Magenta));
        internal static readonly ColourDescription Rose = new(nameof(Rose));

        internal static readonly List<ColourDescription> Lightnesses = new() { Shadow, Dark, Pure, Light, Pale };
        internal static readonly List<ColourDescription> Saturations = new() { Faint, Weak, Mild, Strong, Vibrant };

        internal static readonly List<ColourDescription> Hues = new()
            { Red, Orange, Yellow, Chartreuse, Green, Mint, Cyan, Azure, Blue, Violet, Magenta, Rose };

        internal static readonly List<ColourDescription> Greyscales = new() { Black, Grey, White };

        internal static IEnumerable<ColourDescription> Get(Hsl hsl)
        {
            if (hsl.UseAsNaN) return new List<ColourDescription> { NotApplicable };

            var (h, s, l) = hsl.ConstrainedTriplet;
            switch (l)
            {
                case <= 0: return new List<ColourDescription> { Black };
                case >= 1: return new List<ColourDescription> { White };
            }

            var lightness = l switch
            {
                < 0.20 => Shadow,
                < 0.40 => Dark,
                < 0.60 => Pure,
                < 0.80 => Light,
                _ => Pale
            };

            if (hsl.UseAsGreyscale) return new List<ColourDescription> { lightness, Grey };

            var strength = s switch
            {
                < 0.20 => Faint,
                < 0.40 => Weak,
                < 0.60 => Mild,
                < 0.80 => Strong,
                _ => Vibrant
            };

            var hue = h switch
            {
                < 15 => Red,
                < 45 => Orange,
                < 75 => Yellow,
                < 105 => Chartreuse,
                < 135 => Green,
                < 165 => Mint,
                < 195 => Cyan,
                < 225 => Azure,
                < 255 => Blue,
                < 285 => Violet,
                < 315 => Magenta,
                < 345 => Rose,
                _ => Red
            };

            return new List<ColourDescription> { lightness, strength, hue };
        }

        public override string ToString() => description.ToLower();
    }
}