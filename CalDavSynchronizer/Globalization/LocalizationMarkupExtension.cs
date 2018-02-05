using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace CalDavSynchronizer.Globalization
{
  [ContentProperty(nameof(Text))]
  [MarkupExtensionReturnType(typeof(string))]
  public class LocalizeExtension : MarkupExtension
  {
    public string Text { get; set; }

    public LocalizeExtension()
    {
    }

    public LocalizeExtension(string text)
    {
      Text = text;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return Strings.Localize(Text);
    }
  }
}
