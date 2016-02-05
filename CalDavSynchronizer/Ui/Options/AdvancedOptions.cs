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
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.Ui.Options
{
  public class AdvancedOptions
  {
    private readonly bool _closeConnectionAfterEachRequest;
    private readonly bool _preemptiveAuthentication;
    private readonly ProxyOptions _proxyOptions;
    private readonly MappingConfigurationBase _mappingConfiguration;

    public AdvancedOptions (bool closeConnectionAfterEachRequest, bool preemptiveAuthentication, ProxyOptions proxyOptions, MappingConfigurationBase mappingConfiguration)
    {
      if (proxyOptions == null)
        throw new ArgumentNullException ("proxyOptions");

      _closeConnectionAfterEachRequest = closeConnectionAfterEachRequest;
      _preemptiveAuthentication = preemptiveAuthentication;
      _proxyOptions = proxyOptions;
      _mappingConfiguration = mappingConfiguration;
    }

    public bool CloseConnectionAfterEachRequest
    {
      get { return _closeConnectionAfterEachRequest; }
    }

    public bool PreemptiveAuthentication
    {
      get { return _preemptiveAuthentication; }
    }

    public ProxyOptions ProxyOptions
    {
      get { return _proxyOptions; }
    }

    public MappingConfigurationBase MappingConfiguration
    {
      get { return _mappingConfiguration; }
    }
  }
}