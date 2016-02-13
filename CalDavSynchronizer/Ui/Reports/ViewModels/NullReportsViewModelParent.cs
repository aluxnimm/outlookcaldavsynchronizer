using System;

namespace CalDavSynchronizer.Ui.Reports.ViewModels
{
  class NullReportsViewModelParent : IReportsViewModelParent
  {
    public static readonly IReportsViewModelParent Instance = new NullReportsViewModelParent ();

    private NullReportsViewModelParent ()
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