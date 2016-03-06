using System;
using System.Collections.Generic;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  internal class NullOptionsViewModelFactory : IOptionsViewModelFactory
  {
    public static readonly IOptionsViewModelFactory Instance = new NullOptionsViewModelFactory();

    public List<OptionsViewModelBase> Create (ICollection<CalDavSynchronizer.Contracts.Options> options, bool fixInvalidSettings)
    {
      return new List<OptionsViewModelBase>();
    }
  }
}