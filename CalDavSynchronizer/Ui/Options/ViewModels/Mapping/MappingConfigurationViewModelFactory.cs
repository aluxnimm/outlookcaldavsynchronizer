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
    private readonly IReadOnlyList<string> _availableCategories;

    public MappingConfigurationViewModelFactory (IReadOnlyList<string> availableCategories)
    {
      if (availableCategories == null)
        throw new ArgumentNullException (nameof (availableCategories));
      _availableCategories = availableCategories;
    }

    public EventMappingConfigurationViewModel Create (EventMappingConfiguration configurationElement)
    {
      var eventMappingConfigurationViewModel = new EventMappingConfigurationViewModel(_availableCategories);
      eventMappingConfigurationViewModel.SetOptions (configurationElement);
      return eventMappingConfigurationViewModel;
    }

    public ContactMappingConfigurationViewModel Create (ContactMappingConfiguration configurationElement)
    {
      var contactMappingConfigurationViewModel = new ContactMappingConfigurationViewModel();
      contactMappingConfigurationViewModel.SetOptions (configurationElement);
      return contactMappingConfigurationViewModel;
    }

    public TaskMappingConfigurationViewModel Create (TaskMappingConfiguration configurationElement)
    {
      var taskMappingConfigurationViewModel = new TaskMappingConfigurationViewModel();
      taskMappingConfigurationViewModel.SetOptions (configurationElement);
      return taskMappingConfigurationViewModel;
    }
  }
}
