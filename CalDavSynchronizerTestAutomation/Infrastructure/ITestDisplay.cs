using System;
using System.Reflection;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  internal interface ITestDisplay
  {
    void AddPassed (MethodInfo test);
    void AddFailed (MethodInfo test, Exception x);
  }
}