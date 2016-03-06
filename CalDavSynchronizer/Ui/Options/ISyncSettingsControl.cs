using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation;

namespace CalDavSynchronizer.Ui.Options
{
  public interface ISyncSettingsControl
  {
    SynchronizationMode Mode { get; set; }
    IList<Item<SynchronizationMode>> AvailableSynchronizationModes { get; }
    bool UseSynchronizationTimeRange { get; set; }
  }
}
