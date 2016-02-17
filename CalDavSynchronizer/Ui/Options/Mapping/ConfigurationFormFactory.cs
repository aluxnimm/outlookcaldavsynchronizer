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
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;

namespace CalDavSynchronizer.Ui.Options.Mapping
{
  internal class ConfigurationFormFactory : IConfigurationFormFactory
  {
    private readonly Func<ICalDavDataAccess> _calDavDataAccessFactory;

    public ConfigurationFormFactory (Func<ICalDavDataAccess> calDavDataAccessFactory)
    {
      _calDavDataAccessFactory = calDavDataAccessFactory;
    }

    public IConfigurationForm<T> CreateGenericForm<T> (object configurationElement)
    {
      return new GenericConfigurationForm<T> (configurationElement);
    }

    public IConfigurationForm<EventMappingConfiguration> Create (EventMappingConfiguration configurationElement)
    {
      var form = new EventMappingConfigurationForm (_calDavDataAccessFactory);
      form.Options = configurationElement;
      return form;
    }

    public IConfigurationForm<ContactMappingConfiguration> Create (ContactMappingConfiguration configurationElement)
    {
      var form = new ContactMappingConfigurationForm();
      form.Options = configurationElement;
      return form;
    }
  }
}