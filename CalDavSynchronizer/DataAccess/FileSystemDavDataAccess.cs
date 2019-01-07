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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using CalDavSynchronizer.Ui.ConnectionTests;
using CalDavSynchronizer.Utilities;
using GenSync;
using GenSync.Logging;

namespace CalDavSynchronizer.DataAccess
{
  public class FileSystemDavDataAccess : ICalDavDataAccess, ICardDavDataAccess
  {
    private readonly DirectoryInfo _directory;
    private readonly string _fileExtension;

    public FileSystemDavDataAccess (Uri uri, string fileExtension)
    {
      _fileExtension = fileExtension;
      _directory = new DirectoryInfo(uri.LocalPath);
    }

    public Task<bool> IsResourceCalender()
    {
      return Task.FromResult(true);
    }

    public Task<bool> DoesSupportCalendarQuery()
    {
      return Task.FromResult(true);
    }

    public Task<bool> IsCalendarAccessSupported()
    {
      return Task.FromResult(true);
    }

    public Task<bool> IsCalendarProxySupported(Uri principalUrl)
    {
      return Task.FromResult(false);
    }

    public Task<CalendarOwnerProperties> GetCalendarOwnerPropertiesOrNull()
    {
      return null;
    }

    public Task<AccessPrivileges> GetPrivileges()
    {
      return Task.FromResult (AccessPrivileges.All);
    }

    public Task<ArgbColor?> GetCalendarColorNoThrow()
    {
      return null;
    }

    public Task<bool> SetCalendarColorNoThrow(ArgbColor color)
    {
      return Task.FromResult(true);
    }

    public Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetEventVersions(DateTimeRange? range)
    {
      return GetAllVersions();
    }

    public Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetTodoVersions(DateTimeRange? range)
    {
      return GetEventVersions(range);
    }

    public Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetAllVersions()
    {
      return Task.FromResult<IReadOnlyList<EntityVersion<WebResourceName, string>>> (
         _directory.EnumerateFiles ($"*{_fileExtension}").Select (f => EntityVersion.Create (new WebResourceName (f.Name), f.LastWriteTimeUtc.ToString ("o"))).ToArray ());
    }

    public Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetVersions(IEnumerable<WebResourceName> eventUrls)
    {
      var versions =
        from url in eventUrls
        let path = Path.Combine(_directory.FullName, url.OriginalAbsolutePath)
        where File.Exists(path)
        select EntityVersion.Create(url, File.GetLastWriteTimeUtc(path).ToString("o"));

      return Task.FromResult<IReadOnlyList<EntityVersion<WebResourceName, string>>>(versions.ToArray());
    }

    public Task<IReadOnlyList<EntityWithId<WebResourceName, string>>> GetEntities(IEnumerable<WebResourceName> eventUrls)
    {
      var entities =
        from url in eventUrls
        let path = Path.Combine(_directory.FullName, url.OriginalAbsolutePath)
        where File.Exists(path)
        select EntityWithId.Create(url, File.ReadAllText(path));

      return Task.FromResult<IReadOnlyList<EntityWithId<WebResourceName, string>>>(entities.ToArray());
    }

    public Task<EntityVersion<WebResourceName, string>> CreateEntity(string iCalData, string name)
    {
      var fileName = name + _fileExtension;
      var path = Path.Combine(_directory.FullName, fileName);
      File.WriteAllText(path, iCalData);
      return Task.FromResult(EntityVersion.Create(new WebResourceName(fileName), File.GetLastWriteTimeUtc(path).ToString("o")));
    }

    public Task<bool> TryDeleteEntity(WebResourceName uri, string etag)
    {
      var path = Path.Combine(_directory.FullName, uri.OriginalAbsolutePath);
      if (!File.Exists(path))
        return Task.FromResult(false);

      File.Delete(path);
      return Task.FromResult(true);
    }

    public Task<EntityVersion<WebResourceName, string>> TryUpdateEntity(WebResourceName url, string etag, string iCalData)
    {
      var path = Path.Combine(_directory.FullName, url.OriginalAbsolutePath);
      File.WriteAllText(path, iCalData);
      return Task.FromResult(EntityVersion.Create(url, File.GetLastWriteTimeUtc(path).ToString("o")));
    }

    public Task<bool> DoesSupportWebDavCollectionSync()
    {
      return Task.FromResult(false);
    }

    public Task<(string SyncToken, IReadOnlyList<(WebResourceName Id, string Version)> ChangedOrAddedItems, IReadOnlyList<WebResourceName> DeletedItems)> CollectionSync(string syncTokenOrNull, IGetVersionsLogger logger)
    {
      throw new NotSupportedException();
    }
  }
}