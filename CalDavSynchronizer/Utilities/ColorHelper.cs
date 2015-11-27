using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;
using System.Data;

namespace CalDavSynchronizer.Utilities
{
  internal static class ColorHelper
  {
    public static Color HexToColor (string hexColor)
    {
      string color = hexColor.ToString().Replace ("#", "");
      byte r = Byte.Parse (color.Substring (0, 2), NumberStyles.HexNumber);
      byte g = Byte.Parse (color.Substring (2, 2), NumberStyles.HexNumber);
      byte b = Byte.Parse (color.Substring (4, 2), NumberStyles.HexNumber);
      byte a = (color.Length == 8) ? Byte.Parse (color.Substring(6, 2), NumberStyles.HexNumber) : Convert.ToByte (255);

      return Color.FromArgb (a, r, g, b);
    }
  }
}
