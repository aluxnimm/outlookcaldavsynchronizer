using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  internal interface IManualAssertUi
  {
    bool Assert (string instruction);
  }
}