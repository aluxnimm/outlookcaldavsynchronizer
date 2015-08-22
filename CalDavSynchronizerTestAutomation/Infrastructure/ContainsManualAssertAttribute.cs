using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  [AttributeUsage (AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
  public class ContainsManualAssertAttribute : Attribute
  {
  }
}