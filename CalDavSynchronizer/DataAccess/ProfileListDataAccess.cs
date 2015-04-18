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
using System.IO;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Properties;
using CalDavSynchronizer.Utilities;

namespace CalDavSynchronizer.DataAccess
{
  internal class ProfileListDataAccess : IProfileListDataAccess
  {
    private readonly string _filePath;

    public ProfileListDataAccess (string filePath)
    {
      _filePath = filePath;
    }

    public ProfileEntry[] Load ()
    {
      if (!File.Exists(_filePath))
        return new ProfileEntry[] { };
      else
        return Serializer<ProfileEntry[]>.Deserialize (File.ReadAllText (_filePath));
    }

    public void Save (ProfileEntry[] options)
    {
      if (!Directory.Exists (Path.GetDirectoryName (_filePath)))
        Directory.CreateDirectory (Path.GetDirectoryName (_filePath));

      File.WriteAllText (_filePath, Serializer<ProfileEntry[]>.Serialize (options));
    }
  }
}