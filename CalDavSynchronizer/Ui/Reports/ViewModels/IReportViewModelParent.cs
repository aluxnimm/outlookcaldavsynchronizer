using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Ui.Reports.ViewModels
{
  public interface IReportViewModelParent
  {
    void DiplayAEntity (Guid synchronizationProfileId, string entityId);
    void DiplayBEntity (Guid synchronizationProfileId, string entityId);
  }
}
