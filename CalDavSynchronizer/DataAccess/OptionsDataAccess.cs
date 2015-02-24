// This file is Part of CalDavSynchronizer (https://sourceforge.net/projects/outlookcaldavsynchronizer/)
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
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Properties;
using CalDavSynchronizer.Utilities;

namespace CalDavSynchronizer.DataAccess
{
  internal class OptionsDataAccess : IOptionsDataAccess
  {
    public Options[] LoadOptions ()
    {
      var serializedOptions = Settings.Default.Options;

      if (string.IsNullOrEmpty (serializedOptions))
        return new Options[] { };
      else
        return Serializer<Options[]>.Deserialize (serializedOptions);
    }

    public void SaveOptions (Options[] options)
    {
      Settings.Default.Options = Serializer<Options[]>.Serialize (options);
      Settings.Default.Save();
    }
  }
}