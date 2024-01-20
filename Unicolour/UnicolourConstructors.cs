namespace Wacton.Unicolour
{

    public partial class Unicolour
    {
        /* standard constructors */
        public Unicolour(ColourSpace colourSpace, (double first, double second, double third) tuple,
            double alpha = 1.0) :
            this(colourSpace, tuple.first, tuple.second, tuple.third, alpha)
        {
        }

        public Unicolour(ColourSpace colourSpace, double first, double second, double third, double alpha = 1.0) :
            this(colourSpace, Configuration.Default, first, second, third, alpha)
        {
        }

        public Unicolour(ColourSpace colourSpace, Configuration config,
            (double first, double second, double third) tuple, double alpha = 1.0) :
            this(colourSpace, config, tuple.first, tuple.second, tuple.third, alpha)
        {
        }

        public Unicolour(ColourSpace colourSpace, Configuration config,
            (double first, double second, double third, double alpha) tuple) :
            this(colourSpace, config, tuple.first, tuple.second, tuple.third, tuple.alpha)
        {
        }

        public Unicolour(ColourSpace colourSpace, Configuration config, double first, double second, double third,
            double alpha = 1.0) :
            this(colourSpace, config, ColourHeritage.None, first, second, third, alpha)
        {
        }

        /* special variations for hex  */
        public Unicolour(string hex) : this(Configuration.Default, hex)
        {
        }

        public Unicolour(Configuration config, string hex) : this(ColourSpace.Rgb, config, Utils.ParseColourHex(hex))
        {
        }
    }
}