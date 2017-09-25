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
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.DataAccess
{
  class ColorMappingDataAccess : IColorMappingsDataAccess
  {
    private readonly IOptionDataAccess _optionDataAccess;

    public ColorMappingDataAccess(IOptionDataAccess optionDataAccess)
    {
      if (optionDataAccess == null) throw new ArgumentNullException(nameof(optionDataAccess));
      _optionDataAccess = optionDataAccess;
    }

    public IReadOnlyList<ColorCategoryMapping> Load()
    {
     return (_optionDataAccess.LoadOrNull().MappingConfiguration as EventMappingConfiguration)
        ?.EventColorToCategoryMappings
        ?? new ColorCategoryMapping[0];
    }

    public void Save(IEnumerable<ColorCategoryMapping> mappings)
    {
      _optionDataAccess.Modify(o =>
      {
        if (o.MappingConfiguration is EventMappingConfiguration eventMappingConfiguration)
          eventMappingConfiguration.EventColorToCategoryMappings = mappings.ToArray();
        else
          o.MappingConfiguration = new EventMappingConfiguration {EventColorToCategoryMappings = mappings.ToArray()};
      });
    }
  }
}