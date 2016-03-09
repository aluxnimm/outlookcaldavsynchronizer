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
    private readonly ICurrentOptions _currentOptions;

    public MappingConfigurationViewModelFactory (IReadOnlyList<string> availableCategories, ICurrentOptions currentOptions)
    {
      if (availableCategories == null)
        throw new ArgumentNullException (nameof (availableCategories));
      if (currentOptions == null)
        throw new ArgumentNullException (nameof (currentOptions));
      _availableCategories = availableCategories;
      _currentOptions = currentOptions;
    }

    public EventMappingConfigurationViewModel Create (EventMappingConfiguration configurationElement)
    {
      var eventMappingConfigurationViewModel = new EventMappingConfigurationViewModel(_availableCategories,_currentOptions);
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
