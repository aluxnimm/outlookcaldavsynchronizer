using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Ui;
using GenSync.Logging;
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;
using Rhino.Mocks;

namespace CalDavSynchronizer.UnitTest.Implementation.Events
{
  [TestFixture]
  public class ColorCategoryMapperFixture
  {
    private ColorCategoryMapper _mapper;
    private List<ColorCategoryMapping> _colorMappings = new List<ColorCategoryMapping>();
    private readonly Dictionary<string, OutlookCategory> _outlookCategories = new Dictionary<string, OutlookCategory>();

    [SetUp]
    public void CreateMapper()
    {
      _mapper = new ColorCategoryMapper(new TestOutlookSession(this), new TestColorMappingsDataAccess(this));
    }

    [Test, TestCaseSource(typeof(ColorMapper), nameof(ColorMapper.HtmlColorNames))]
    public void TestRoundtrip2To1(string originalhtmlColor)
    {
      var category = _mapper.MapHtmlColorToCategoryOrNull(originalhtmlColor, NullEntitySynchronizationLogger.Instance);
      var roundTrippedHtmlColor = _mapper.MapCategoryToHtmlColorOrNull(category);
      Assert.That(roundTrippedHtmlColor, Is.EqualTo(originalhtmlColor));

      // try a second roundtrip
      category = _mapper.MapHtmlColorToCategoryOrNull(originalhtmlColor, NullEntitySynchronizationLogger.Instance);
      roundTrippedHtmlColor = _mapper.MapCategoryToHtmlColorOrNull(category);
      Assert.That(roundTrippedHtmlColor, Is.EqualTo(originalhtmlColor));
    }

    [Test, TestCaseSource(nameof(GetTestRoundtrip1To2TestCases))]
    public void TestRoundtrip1To2(OlCategoryColor outlookColor)
    {
      var originalCategory = "cat1";
      _outlookCategories[originalCategory] = new OutlookCategory(originalCategory, outlookColor, OlCategoryShortcutKey.olCategoryShortcutKeyNone);
      CreateMapper();

      var htmlColor = _mapper.MapCategoryToHtmlColorOrNull(originalCategory);
      var roundTrippedCategory = _mapper.MapHtmlColorToCategoryOrNull(htmlColor, NullEntitySynchronizationLogger.Instance);
      Assert.That(roundTrippedCategory, Is.EqualTo(originalCategory));

      // try a second roundtrip
      htmlColor = _mapper.MapCategoryToHtmlColorOrNull(originalCategory);
      roundTrippedCategory = _mapper.MapHtmlColorToCategoryOrNull(htmlColor, NullEntitySynchronizationLogger.Instance);
      Assert.That(roundTrippedCategory, Is.EqualTo(originalCategory));
    }

    [TestCase("#ff0000","red",OlCategoryColor.olCategoryColorRed)]
    [TestCase("#ff00ff", "purple", OlCategoryColor.olCategoryColorPurple)]
    public void TestRoundtrip2To1WithNonStandardHtmlColorName(string originalhtmlColor, string expectedRoundTrippedHtmlColor, OlCategoryColor expectedOutlookColor)
    {
      var category = _mapper.MapHtmlColorToCategoryOrNull(originalhtmlColor, NullEntitySynchronizationLogger.Instance);
      var roundTrippedHtmlColor = _mapper.MapCategoryToHtmlColorOrNull(category);
      Assert.That(roundTrippedHtmlColor, Is.EqualTo(expectedRoundTrippedHtmlColor));
      Assert.That(_outlookCategories[category].Color, Is.EqualTo(expectedOutlookColor));

      category = _mapper.MapHtmlColorToCategoryOrNull(originalhtmlColor, NullEntitySynchronizationLogger.Instance);
      roundTrippedHtmlColor = _mapper.MapCategoryToHtmlColorOrNull(category);
      Assert.That(roundTrippedHtmlColor, Is.EqualTo(expectedRoundTrippedHtmlColor));
      Assert.That(_outlookCategories[category].Color, Is.EqualTo(expectedOutlookColor));
    }


    static IEnumerable<OlCategoryColor> GetTestRoundtrip1To2TestCases()
    {
      return Enum.GetValues(typeof(OlCategoryColor)).Cast<OlCategoryColor>().Where(e => e != OlCategoryColor.olCategoryColorNone);
    }

    class TestColorMappingsDataAccess : IColorMappingsDataAccess
    {
      private readonly ColorCategoryMapperFixture _parent;

      public TestColorMappingsDataAccess(ColorCategoryMapperFixture parent)
      {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
      }

      public IReadOnlyList<ColorCategoryMapping> Load()
      {
        return _parent._colorMappings;
      }

      public void Save(IEnumerable<ColorCategoryMapping> mappings)
      {
        _parent._colorMappings = mappings.ToList();
      }
    }

    class TestOutlookSession : IOutlookSession
    {
      private readonly ColorCategoryMapperFixture _parent;

      public TestOutlookSession(ColorCategoryMapperFixture parent)
      {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
      }

      public string ApplicationVersion { get; }
      public IOutlookTimeZones TimeZones { get; }
      public StringComparer CategoryNameComparer { get; }

      public IReadOnlyCollection<OutlookCategory> GetCategories()
      {
        return _parent._outlookCategories.Values.ToArray();
      }

      public string GetCurrentUserEmailAddressOrNull()
      {
        throw new NotImplementedException();
      }

      public OutlookFolderDescriptor GetFolderDescriptorFromId(string entryId, object storeId)
      {
        throw new NotImplementedException();
      }

      public Folder GetFolderFromId(string entryId, object storeId)
      {
        throw new NotImplementedException();
      }

      public AppointmentItem GetAppointmentItemOrNull(string entryId, string expectedFolderId, string storeId)
      {
        throw new NotImplementedException();
      }

      public TaskItem GetTaskItemOrNull(string entryId, string expectedFolderId, string storeId)
      {
        throw new NotImplementedException();
      }

      public ContactItem GetContactItemOrNull(string entryId, string expectedFolderId, string storeId)
      {
        throw new NotImplementedException();
      }

      public DistListItem GetDistListItemOrNull(string entryId, string expectedFolderId, string storeId)
      {
        throw new NotImplementedException();
      }

      public AppointmentItem GetAppointmentItem(string entryId, string storeId)
      {
        throw new NotImplementedException();
      }

      public AppointmentItem GetAppointmentItem(string entryId)
      {
        throw new NotImplementedException();
      }

      public TaskItem GetTaskItem(string entryId, string storeId)
      {
        throw new NotImplementedException();
      }

      public ContactItem GetContactItem(string entryId, string storeId)
      {
        throw new NotImplementedException();
      }

      public DistListItem GetDistListItem(string entryId, string storeId)
      {
        throw new NotImplementedException();
      }

      public Recipient CreateRecipient(string recipientName)
      {
        throw new NotImplementedException();
      }

      public CreateCategoryResult AddCategoryNoThrow(string name, OlCategoryColor color)
      {
        _parent._outlookCategories.Add(name, new OutlookCategory (name, color, OlCategoryShortcutKey.olCategoryShortcutKeyNone));
        return CreateCategoryResult.Ok;
      }

      public void AddOrUpdateCategoryNoThrow(string name, OlCategoryColor color, bool useColor, OlCategoryShortcutKey shortcutKey, bool useShortcutKey)
      {
        throw new NotImplementedException();
      }

      public IReadOnlyDictionary<string, IReadOnlyList<OutlookFolderDescriptor>> GetFoldersByName()
      {
        throw new NotImplementedException();
      }
    }
  }
}
