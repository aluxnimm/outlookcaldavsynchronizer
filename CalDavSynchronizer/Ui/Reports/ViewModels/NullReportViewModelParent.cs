using System;

namespace CalDavSynchronizer.Ui.Reports.ViewModels
{
  class NullReportViewModelParent : IReportViewModelParent
  {
    public static readonly IReportViewModelParent Instance = new NullReportViewModelParent();

    private NullReportViewModelParent ()
    {
    }

    public void DiplayAEntity (Guid synchronizationProfileId, string entityId)
    {
      
    }

    public void DiplayBEntity (Guid synchronizationProfileId, string entityId)
    {
    
    }
  }
}