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
using log4net;

namespace CalDavSynchronizer.DataAccess
{
  class OptionDataAccess : IOptionDataAccess
  {
    private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly Guid _profileId;
    private readonly IOptionsDataAccess _optionsDataAccess;

    public OptionDataAccess(Guid profileId, IOptionsDataAccess optionsDataAccess)
    {
      if (optionsDataAccess == null) throw new ArgumentNullException(nameof(optionsDataAccess));
      _profileId = profileId;
      _optionsDataAccess = optionsDataAccess;
    }

    public Options LoadOrNull()
    {
      var options = _optionsDataAccess.Load();
      return options.FirstOrDefault(o => o.Id == _profileId);
    }

    public void Modify(Action<Options> modifier)
    {
      s_logger.Debug("Entered");
      var options = _optionsDataAccess.Load();
      var profileOptions = options.FirstOrDefault(o => o.Id == _profileId);

      if (profileOptions == null)
      {
        s_logger.Error($"Cannot modify options for profile '{_profileId}', because it doesn't exist");
        return;
      }

      modifier(profileOptions);
      _optionsDataAccess.Save(options);
      s_logger.Debug("Exiting");
    }
  }
}
