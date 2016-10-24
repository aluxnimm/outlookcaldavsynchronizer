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
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Ui.Options;
using Microsoft.Office.Interop.Outlook;
using CalDavSynchronizer.Utilities;

namespace CalDavSynchronizer
{
  class ProfileExportProcessor : IProfileExportProcessor
  {
    private readonly NameSpace _session;
    private readonly IOptionTasks _optionTasks;

    public ProfileExportProcessor (NameSpace session, IOptionTasks optionTasks)
    {
      if (session == null) throw new ArgumentNullException(nameof(session));
      if (optionTasks == null) throw new ArgumentNullException(nameof(optionTasks));

      _session = session;
      _optionTasks = optionTasks;
    }

    public void PrepareForExport(Options[] profiles, Action<string> logger)
    {
      logger("Processing profiles.");

      foreach (var profile in profiles)
      {
        logger($"'{profile.Name}'");
        try
        {
          using (var outlookFolderWrapper = GenericComObjectWrapper.Create((Folder) _session.GetFolderFromID(profile.OutlookFolderEntryId, profile.OutlookFolderStoreId)))
          {
            profile.OutlookFolderEntryId = outlookFolderWrapper.Inner.Name;
          }
        }
        catch (System.Exception)
        {
          logger ($"WARNING profile '{profile.Name}', references an outlook folder that doesn't exist.");
          profile.OutlookFolderEntryId = "<ERROR>";
        }
        profile.OutlookFolderStoreId = null;
      }

      logger ("Processing profiles done.");
    }

    public Options[] PrepareAndMergeForImport (Options[] existingProfiles,Options[] profilesToImport, Action<string> logger)
    {
      PrepareForImport(profilesToImport, logger);
      return MergePreservingSortOrder(existingProfiles, profilesToImport, logger);
    }

    private void PrepareForImport(Options[] options, Action<string> logger)
    {
      var folderIdsByName = new Dictionary<string, List<Tuple<string, string>>>();
      AddFoldersRecusive(_session.Folders, folderIdsByName);

      foreach (var profile in options)
      {
        var ids = folderIdsByName.GetOrAdd(profile.OutlookFolderEntryId).FirstOrDefault();
        if (ids != null)
        {
          profile.OutlookFolderEntryId = ids.Item1;
          profile.OutlookFolderStoreId = ids.Item2;

          profile.OutlookFolderAccountName = _optionTasks.GetFolderAccountNameOrNull(profile.OutlookFolderStoreId);
        }
        else
        {
          logger($"Warning: did not find Folder '{profile.OutlookFolderEntryId}'");
          profile.OutlookFolderEntryId = null;
          profile.OutlookFolderStoreId = null;
          profile.OutlookFolderAccountName = null;
        }
      }
    }

    private void AddFoldersRecusive(Folders folders, Dictionary<string, List<Tuple<string, string>>> collector)
    {
      // TODO: do not add "Deleted Items"-Folder
      foreach (var folder in folders.Cast<Folder> ().ToSafeEnumerable ())
      {
        AddFoldersRecusive(folder.Folders, collector);
        collector.GetOrAdd(folder.Name).Add(Tuple.Create(folder.EntryID, folder.StoreID));
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
          logger($"Adding profile '{profileToImport.Name}'");
          mergedProfiles.Add (profileToImport);
        }
        else
        {
          logger ($"Replacing profile '{profileToImport.Name}'");
          var index = mergedProfiles.IndexOf (existingProfile);
          mergedProfiles.RemoveAt (index);
          mergedProfiles.Insert (index, profileToImport);
        }
      }

      return mergedProfiles.ToArray ();
    }
  }
}