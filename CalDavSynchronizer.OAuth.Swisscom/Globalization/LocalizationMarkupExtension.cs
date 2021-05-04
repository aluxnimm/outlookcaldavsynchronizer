using System;
using System.Windows.Markup;

namespace CalDavSynchronizer.OAuth.Swisscom.Globalization
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
