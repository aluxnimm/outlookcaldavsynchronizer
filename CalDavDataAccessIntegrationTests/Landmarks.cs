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
using NUnit.Framework;

namespace CalDavDataAccessIntegrationTests
{
  public class Landmarks : FixtureBase
  {
    protected override string ProfileName
    {
      get { return "Landmarks"; }
    }

    [Ignore ("Landmarks doesnt create a new entity in that case, it fails with precondition.")]
    public override async System.Threading.Tasks.Task UpdateNonExistingEntity_CreatesNewEntity ()
    {
      await base.UpdateNonExistingEntity_CreatesNewEntity ();
    }

     [Ignore ("Landmarks doesnt fail with 404 in that case, it fails with precondition.")]
    public override async System.Threading.Tasks.Task DeleteNonExistingEntity_ThrowsNotFound ()
    {
      await base.DeleteNonExistingEntity_ThrowsNotFound ();
    } 
  }
}