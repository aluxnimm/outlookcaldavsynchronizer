using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using CalDavSynchronizer.Utilities;
using GenSync;

namespace CalDavSynchronizer.DataAccess
{
  public class FileSystemCalDavDataAccess : ICalDavDataAccess
  {
    private readonly DirectoryInfo _directory;

    public FileSystemCalDavDataAccess(Uri uri)
    {
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

    public Task<bool> IsWriteable()
    {
      return Task.FromResult(true);
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
      return Task.FromResult<IReadOnlyList<EntityVersion<WebResourceName, string>>>(
        _directory.EnumerateFiles().Select(f => EntityVersion.Create(new WebResourceName(f.Name), f.LastWriteTimeUtc.ToString("o"))).ToArray());
    }

    public Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetTodoVersions(DateTimeRange? range)
    {
      return GetEventVersions(range);
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

    public Task<EntityVersion<WebResourceName, string>> CreateEntity(string iCalData, string uid)
    {
      var fileName = uid + ".ics";
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
  }
}