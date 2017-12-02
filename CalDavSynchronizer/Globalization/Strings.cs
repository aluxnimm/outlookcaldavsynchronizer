using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Globalization
{
  public class Strings
  {
    public static string Get(FormattableString key)
    {
      return string.Format(Localize(key.Format), key.GetArguments());
    }

    public static string Localize(string text)
    {
      return Regex.Replace(text,"[A-Za-z]","_");
    }
  }
}
