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
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Ui.Options;
using Microsoft.Office.Interop.Outlook;
using CalDavSynchronizer.Utilities;

namespace CalDavSynchronizer
{
  class ProfileExportProcessor : IProfileExportProcessor
  {
    private readonly IOutlookSession _session;
    private readonly IOptionTasks _optionTasks;

    public ProfileExportProcessor(IOutlookSession session, IOptionTasks optionTasks)
    {
      if (session == null) throw new ArgumentNullException(nameof(session));
      if (optionTasks == null) throw new ArgumentNullException(nameof(optionTasks));

      _session = session;
      _optionTasks = optionTasks;
    }

    public void PrepareForExport(Options[] profiles, Action<string> logger)
    {
      logger(Strings.Get($"Processing profiles."));

      foreach (var profile in profiles)
      {
        logger($"'{profile.Name}'");
        try
        {
          using (var outlookFolderWrapper = GenericComObjectWrapper.Create((Folder) _session.GetFolderFromId(profile.OutlookFolderEntryId, profile.OutlookFolderStoreId)))
          {
            profile.OutlookFolderEntryId = outlookFolderWrapper.Inner.Name;
          }
        }
        catch (System.Exception)
        {
          logger (Strings.Get($"WARNING profile '{profile.Name}', references an outlook folder that doesn't exist."));
          profile.OutlookFolderEntryId = "<ERROR>";
        }
        profile.OutlookFolderStoreId = null;
      }

      logger (Strings.Get($"Processing profiles done."));
    }

    public Options[] PrepareAndMergeForImport (Options[] existingProfiles,Options[] profilesToImport, Action<string> logger)
    {
      PrepareForImport(profilesToImport, logger);
      return MergePreservingSortOrder(existingProfiles, profilesToImport, logger);
    }

    private void PrepareForImport(Options[] options, Action<string> logger)
    {
      var folderIdsByName = _session.GetFoldersByName();

      foreach (var profile in options)
      {
        var folder = folderIdsByName.GetOrDefault(profile.OutlookFolderEntryId)?.FirstOrDefault();
        if (folder != null)
        {
          profile.OutlookFolderEntryId = folder.EntryId;
          profile.OutlookFolderStoreId = folder.StoreId;

          profile.OutlookFolderAccountName = _optionTasks.GetFolderAccountNameOrNull(profile.OutlookFolderStoreId);
        }
        else
        {
          logger(Strings.Get($"Warning: did not find folder '{profile.OutlookFolderEntryId}'"));
          profile.OutlookFolderEntryId = null;
          profile.OutlookFolderStoreId = null;
          profile.OutlookFolderAccountName = null;
        }
      }
    }
    private Options[] MergePreservingSortOrder (Options[] existingProfiles, Options[] profilesToImport, Action<string> logger)
    {
      var mergedProfiles = existingProfiles.ToList ();

      foreach (var profileToImport in profilesToImport)
      {
        var existingProfile = mergedProfiles.FirstOrDefault (p => p.Id == profileToImport.Id);
        if (existingProfile == null)
        {
          logger(Strings.Get($"Adding profile '{profileToImport.Name}'"));
          mergedProfiles.Add (profileToImport);
        }
        else
        {
          logger (Strings.Get($"Replacing profile '{profileToImport.Name}'"));
          var index = mergedProfiles.IndexOf (existingProfile);
          mergedProfiles.RemoveAt (index);
          mergedProfiles.Insert (index, profileToImport);
        }
      }

      return mergedProfiles.ToArray ();
    }
  }
}