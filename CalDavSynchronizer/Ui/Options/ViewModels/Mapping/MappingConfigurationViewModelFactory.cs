// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
      var taskMappingConfigurationViewModel = new TaskMappingConfigurationViewModel (_availableCategories);
      taskMappingConfigurationViewModel.SetOptions (configurationElement);
      return taskMappingConfigurationViewModel;
    }
  }
}
