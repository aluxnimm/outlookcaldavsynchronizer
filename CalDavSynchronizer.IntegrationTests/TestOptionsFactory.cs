using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;

namespace CalDavSynchronizer.IntegrationTests
{
  public static class TestOptionsFactory
  {
    public static Options CreateSogoEvents()
    {
      var options = CreateDefaultOptions("IntegrationTest/Events/Sogo");
      options.MappingConfiguration = CreateDefaultEventMappingConfiguration();
      return options;
    }

    public static Options CreateGoogleEvents()
    {
      var options = CreateDefaultOptions("IntegrationTest/Events/Google");
      options.MappingConfiguration = CreateDefaultEventMappingConfiguration();
      return options;
    }

    public static Options CreateSogoContacts()
    {
      var options = CreateDefaultOptions("IntegrationTests/Contacts/Sogo");
      options.MappingConfiguration = CreateDefaultContactMappingConfiguration();
      return options;
    }

    public static Options CreateGoogleContacts()
    {
      var options = CreateDefaultOptions("IntegrationTests/Contacts/Google");
      options.MappingConfiguration = CreateDefaultContactMappingConfiguration();
      return options;
    }

    public static Options CreateSogoTasks()
    {
      var options = CreateDefaultOptions("IntegrationTest/Tasks/Sogo");
      options.MappingConfiguration = CreateDefaultTaskMappingConfiguration();
      return options;
    }

    public static Options CreateGoogleTasks()
    {
      var options = CreateDefaultOptions("IntegrationTest/Tasks/Google");
      options.MappingConfiguration = CreateDefaultTaskMappingConfiguration();
      return options;
    }

    private static EventMappingConfiguration CreateDefaultEventMappingConfiguration()
    {
      return new EventMappingConfiguration();
    }

    private static MappingConfigurationBase CreateDefaultContactMappingConfiguration()
    {
      return new ContactMappingConfiguration();
    }

    private static MappingConfigurationBase CreateDefaultTaskMappingConfiguration()
    {
      return new TaskMappingConfiguration();
    }

    public static Options CreateDefaultOptions(string profileName)
    {
      var applicationDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CalDavSynchronizer");

      var optionsDataAccess = new OptionsDataAccess(ComponentContainer.GetOrCreateDataDirectory(applicationDataDirectory, "Outlook").ConfigFilePath);

      var options = optionsDataAccess.Load().Single(o => o.Name == profileName);
      
      return new Options
      {
        ProtectedPassword = options.ProtectedPassword,
        Salt = options.Salt,
        UserName = options.UserName,
        CalenderUrl = options.CalenderUrl,
        OutlookFolderEntryId = options.OutlookFolderEntryId,
        OutlookFolderStoreId = options.OutlookFolderStoreId,
        EmailAddress = options.EmailAddress,
        ServerAdapterType = options.ServerAdapterType,

        IsChunkedSynchronizationEnabled = false,
        ChunkSize = 100,
        IgnoreSynchronizationTimeRange = true,
        SynchronizationMode = Implementation.SynchronizationMode.MergeInBothDirections,
        ConflictResolution = Implementation.ConflictResolution.Automatic,
        SynchronizationIntervalInMinutes = 0,
      };
    }
  }
}
