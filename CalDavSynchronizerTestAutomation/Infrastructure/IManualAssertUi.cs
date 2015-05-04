using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  interface IManualAssertUi
  {
    bool Assert (string instruction);
  }
}
