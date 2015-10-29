using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Implementation
{
  public interface IOutlookRepository
  {
    bool IsResponsibleForFolder (string folderEntryId, string folderStoreId);
  }
}