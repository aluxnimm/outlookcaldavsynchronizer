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
using System.Reflection;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.EntityMapping;
using GenSync.Logging;
using Google.Contacts;
using log4net;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  public class GoogleContactEntityMapper : IEntityMapper<ContactItemWrapper, Contact>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ContactMappingConfiguration _configuration;

    public GoogleContactEntityMapper (ContactMappingConfiguration configuration)
    {
      _configuration = configuration;
    }

    public Contact Map1To2 (ContactItemWrapper source, Contact target, IEntityMappingLogger logger)
    {
      target.Name.GivenName = source.Inner.FirstName;
      target.Name.FamilyName = source.Inner.LastName;
      return target;
    }

    public ContactItemWrapper Map2To1 (Contact source, ContactItemWrapper target, IEntityMappingLogger logger)
    {
      target.Inner.FirstName = source.Name.GivenName;
      target.Inner.LastName = source.Name.FamilyName;

      return target;
    }
  }
}