using GenSync.ProgressReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Ui
{
    internal class NullGeneralProgressUi : IGeneralProgressUi
    {
        public void Close()
        {
        }

        public void SetProgressValue(int percent)
        {
        }
    }
}
