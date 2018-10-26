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
using CalDavSynchronizer.Utilities;

namespace CalDavSynchronizer.DataAccess
{
  public class CalendarData
  {
    public Uri Uri { get; }
    public string Name { get; }
    public ArgbColor? Color { get; }
    public bool ReadOnly { get; }
    public bool IsDefault { get; }

    public CalendarData (Uri uri, string name, ArgbColor? color, bool readOnly = false, bool isDefault = false)
    {
      Uri = uri;
      Name = name;
      Color = color;
      ReadOnly = readOnly;
      IsDefault = isDefault;
    }
  }
}