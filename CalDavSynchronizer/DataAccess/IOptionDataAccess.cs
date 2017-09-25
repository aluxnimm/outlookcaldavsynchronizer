using System;
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.DataAccess
{
  internal interface IOptionDataAccess
  {
    Options LoadOrNull();
    void Modify(Action<Options> modifier);
  }
}