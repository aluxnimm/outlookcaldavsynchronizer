using System;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.Models
{
  public class OneTimeChangeCategoryTask
  {
    public OneTimeChangeCategoryTask(string categoryName, OlCategoryColor? eventCategoryColor, OlCategoryShortcutKey? categoryShortcutKey)
    {
      CategoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
      EventCategoryColor = eventCategoryColor;
      CategoryShortcutKey = categoryShortcutKey;
    }

    public string CategoryName { get; }
    public OlCategoryColor? EventCategoryColor { get; }
    public OlCategoryShortcutKey? CategoryShortcutKey { get; }
  }
}