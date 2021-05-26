namespace CalDavSynchronizer.ProfileTypes.ConcreteTypes.Daimler.Helpers
{
    public static class Extensions
    {
        public static string EnsureEndsWith(this string input, string end = default)
        {
            if (string.IsNullOrEmpty(end)) return input;

            if (string.IsNullOrEmpty(input)) return end;

            if (input.EndsWith(end)) return input;

            return $"{input}{end}";
        }
    }
}
