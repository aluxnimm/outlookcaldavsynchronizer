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
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.Ui.Options.ProfileTypes
{
  public class ProfileTypeRegistry : IProfileTypeRegistry
  {
    public static readonly IProfileTypeRegistry Instance = Create();

    private readonly GenericProfile _genericProfile;
    private readonly GoogleProfile _googleProfile;

    private ProfileTypeRegistry(IReadOnlyList<IProfileType> allTypes, GenericProfile genericProfile, GoogleProfile googleProfile)
    {
      _genericProfile = genericProfile;
      _googleProfile = googleProfile;
      AllTypes = allTypes;
    }

    private static IProfileTypeRegistry Create()
    {
      var generic = new GenericProfile();
      var google = new GoogleProfile();
      var all = new List<IProfileType> { generic, google };
      all.Add(new ContactsiCloudProfile());
      all.Add(new FruuxProfile());
      all.Add(new PosteoProfile());
      all.Add(new YandexProfile());
      all.Add(new GmxCalendarProfile());
      all.Add(new SarenetProfile());
      all.Add(new LandmarksProfile());
      all.Add(new SogoProfile());
      all.Add(new CozyProfile());
      all.Add(new NextcloudProfile());
      all.Add(new MailboxOrgProfile());
      all.Add(new EasyProjectProfile());
      all.Add(new WebDeProfile());
      all.Add(new SmarterMailProfile());
      all.Add(new MailDeProfile());

      return new ProfileTypeRegistry(all, generic, google);
    }

    public IReadOnlyList<IProfileType> AllTypes { get; }

    public IProfileType DetermineType(Contracts.Options data)
    {
      if (_googleProfile.IsGoogleProfile(data))
        return _googleProfile;
      else
        return _genericProfile;
    }
  }
}