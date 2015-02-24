// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using CalDavSynchronizer.EntityVersionManagement;
using NUnit.Framework;

namespace CalDavSynchronizer.UnitTest.EntityVersionManagement
{
  [TestFixture]
  public class VersionStorageFixture
  {
    [Test]
    public void SetNewVersions_ComplexScenario ()
    {
      var versionStorage = CreateSsystemUnderTest();

      var delta = versionStorage.SetNewVersions (new[]
                                             {
                                                 new EntityIdWithVersion<int, int> (1, 1),
                                                 new EntityIdWithVersion<int, int> (2, 1),
                                                 new EntityIdWithVersion<int, int> (3, 1),
                                                 new EntityIdWithVersion<int, int> (4, 1),
                                             });

      Assert.AreEqual (4, delta.Added.Count);

      delta = versionStorage.SetNewVersions (new[]
                                         {
                                             new EntityIdWithVersion<int, int> (2, 1),
                                             new EntityIdWithVersion<int, int> (3, 2),
                                             new EntityIdWithVersion<int, int> (4, 1),
                                             new EntityIdWithVersion<int, int> (5, 1),
                                         });

      Assert.AreEqual (1, delta.Added.Count);
      Assert.AreEqual (5, delta.Added[0].Id);
      Assert.AreEqual (1, delta.Added[0].Version);

      Assert.AreEqual (1, delta.Deleted.Count);
      Assert.AreEqual (1, delta.Deleted[0].Id);
      Assert.AreEqual (1, delta.Deleted[0].Version);

      Assert.AreEqual (1, delta.Changed.Count);
      Assert.AreEqual (3, delta.Changed[0].Id);
      Assert.AreEqual (2, delta.Changed[0].Version);

    
    }


    private static VersionStorage<int, int> CreateSsystemUnderTest ()
    {
      var versionStorage = new VersionStorage<int, int> ();
      return versionStorage;
    }
  }
}