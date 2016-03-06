using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.Ui.Options.ViewModels.Mapping
{
  class MappingConfigurationViewModelFactory : IMappingConfigurationViewModelFactory
  {
    public static readonly IMappingConfigurationViewModelFactory Instance = new MappingConfigurationViewModelFactory ();

    private MappingConfigurationViewModelFactory ()
    {
    }

    public EventMappingConfigurationViewModel Create (EventMappingConfiguration configurationElement)
    {
      return new EventMappingConfigurationViewModel();
    }

    public ContactMappingConfigurationViewModel Create (ContactMappingConfiguration configurationElement)
    {
      return new ContactMappingConfigurationViewModel();
    }

    public TaskMappingConfigurationViewModel Create (TaskMappingConfiguration configurationElement)
    {
      return new TaskMappingConfigurationViewModel();
    }
  }
}
