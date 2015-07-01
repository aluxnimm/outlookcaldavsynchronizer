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

namespace CalDavSynchronizer.UnitTest.InitialEntityMatching
{
  public class PersonB
  {
    private readonly Identifier<string> _id;
    private readonly string _age;
    private readonly string _name;
    private readonly string _version;

    public PersonB (Identifier<string> id, string name, string age, string version)
    {
      _id = id;
      _name = name;
      _age = age;
      _version = version;
    }


    public Identifier<string> Id
    {
      get { return _id; }
    }

    public string Age
    {
      get { return _age; }
    }

    public string Name
    {
      get { return _name; }
    }

    public string Version
    {
      get { return _version; }
    }
  }
}