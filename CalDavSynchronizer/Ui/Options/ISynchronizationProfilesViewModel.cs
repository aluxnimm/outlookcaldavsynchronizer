using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Ui.Options
{
  interface ISynchronizationProfilesViewModel
  {
    void ShowProfile (Guid value);
    void BringToFront ();
  }
}
