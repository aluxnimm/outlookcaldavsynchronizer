using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;

namespace CalDavSynchronizer.IntegrationTests.Infrastructure
{
  class InMemoryGeneralOptionsDataAccess : IGeneralOptionsDataAccess
  {
    private GeneralOptions _options;

    public GeneralOptions LoadOptions()
    {
      return (_options ??
             (_options = new GeneralOptions
             {
               CalDavConnectTimeout = TimeSpan.FromSeconds(10),
               MaxReportAgeInDays = 100,
                QueryFoldersJustByGetTable = true
             }))
             .Clone();
    }

    public void SaveOptions(GeneralOptions options)
    {
      _options = options.Clone();
    }

    public Version IgnoreUpdatesTilVersion { get; set; }

    public int EntityCacheVersion
    {
      get { return ComponentContainerTestExtensioncs.GetRequiredEntityCacheVersion(); }
      set { }
    }

    public bool GoogleProfilesConvertedToConfigurableChunkSize
    {
      get { return true; }
      set { }
    }
  }
}
