namespace Wacton.Unicolour
{

    internal record ColourHeritage(string description)
    {
        private readonly string description = description;

        internal static readonly ColourHeritage None = new(nameof(None));
        internal static readonly ColourHeritage NaN = new(nameof(NaN));
        internal static readonly ColourHeritage Greyscale = new(nameof(Greyscale));
        internal static readonly ColourHeritage Hued = new(nameof(Hued));
        internal static readonly ColourHeritage GreyscaleAndHued = new(nameof(GreyscaleAndHued));

        internal static ColourHeritage From(ColourRepresentation parent)
        {
            if (parent.UseAsNaN) return NaN;
            return parent switch
            {
                { UseAsGreyscale: true, UseAsHued: false } => Greyscale,
                { UseAsGreyscale: false, UseAsHued: true } => Hued,
                { UseAsGreyscale: true, UseAsHued: true } => GreyscaleAndHued,
                _ => None
            };
        }

        internal static ColourHeritage From(ColourRepresentation firstParent, ColourRepresentation secondParent)
        {
            var first = From(firstParent);
            var second = From(secondParent);

            var eitherNaN = first == NaN || second == NaN;
            var bothGreyscale = (first == Greyscale || first == GreyscaleAndHued) &&
                                (second == Greyscale || second == GreyscaleAndHued);
            var eitherHued = first == Hued || first == GreyscaleAndHued || second == Hued || second == GreyscaleAndHued;

            if (eitherNaN) return NaN;
            if (bothGreyscale)
            {
                return eitherHued ? GreyscaleAndHued : Greyscale;
            }

            return eitherHued ? Hued : None;
        }

        public override string ToString() => description;
    }
}