using System;

namespace CalDavSynchronizer.IntegrationTests.TestBase.ComWrappers
{
  abstract class TestComWrapperBase<TDerived> : IDisposable
  {
    private bool _isDisposed;
    private readonly Action<TDerived> _onDisposed;

    protected TestComWrapperBase(Action<TDerived> onDisposed)
    {
      _onDisposed = onDisposed;
    }

    public void Dispose()
    {
      DisposeOverride();

      if (_isDisposed)
        return;
      _isDisposed = true;
      _onDisposed(This());
    }

    protected abstract void DisposeOverride();
    protected abstract TDerived This();
  }
}