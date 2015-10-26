using System;
using System.Reflection;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  internal interface ITestDisplay
  {
    void SetRunPending (MethodInfo test);
    void SetPassed (MethodInfo test);
    void SetFailed (MethodInfo test, Exception x);
  }
}