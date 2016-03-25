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
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options
{
  public class OptionsDisplayControlFactory : IOptionsDisplayControlFactory
  {
    private readonly NameSpace _session;
    private readonly Func<Guid, string> _profileDataDirectoryFactory;
    private readonly GeneralOptions _generalOptions;
    private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;

    public OptionsDisplayControlFactory (
      NameSpace session, 
      Func<Guid, string> profileDataDirectoryFactory,
      GeneralOptions generalOptions, 
      IOutlookAccountPasswordProvider outlookAccountPasswordProvider)
    {
      if (session == null)
        throw new ArgumentNullException (nameof (session));
      if (profileDataDirectoryFactory == null)
        throw new ArgumentNullException (nameof (profileDataDirectoryFactory));
      if (generalOptions == null)
        throw new ArgumentNullException (nameof (generalOptions));
      if (outlookAccountPasswordProvider == null)
        throw new ArgumentNullException (nameof (outlookAccountPasswordProvider));

      _session = session;
      _profileDataDirectoryFactory = profileDataDirectoryFactory;
      _generalOptions = generalOptions;
      _outlookAccountPasswordProvider = outlookAccountPasswordProvider;
    }

    public IOptionsDisplayControl Create (Contracts.Options options)
    {
      if (options.ServerAdapterType == ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth
          || options.ServerAdapterType == ServerAdapterType.GoogleTaskApi)
        return new GoogleOptionsDisplayControl (_session, _profileDataDirectoryFactory, _generalOptions);

      return new OptionsDisplayControl (
          _session,
          _profileDataDirectoryFactory,
          _generalOptions,
          _outlookAccountPasswordProvider);
    }
  }
}