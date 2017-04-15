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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Scheduling;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.IntegrationTests.Infrastructure
{
  public static class ComponentContainerTestExtensioncs
  {
    public static SynchronizerFactory GetSynchronizerFactory(this ComponentContainer componentContainer)
    {
      // ReSharper disable once PossibleNullReferenceException
      return (SynchronizerFactory) componentContainer
        .GetType()
        .GetField("_synchronizerFactory", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        .GetValue(componentContainer);
    }

    public static int GetRequiredEntityCacheVersion()
    {
      // ReSharper disable once PossibleNullReferenceException
      return (int)
        typeof(ComponentContainer)
          .GetField("c_requiredEntityCacheVersion", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
          .GetValue(null);
    }


    public static string GetProfileDataDirectory (this ComponentContainer componentContainer, Guid profileId)
    {
      return (string) componentContainer
        .GetType ()
        .GetMethod ("GetProfileDataDirectory", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        .Invoke (componentContainer, new object[] { profileId });
    }
  }
}
