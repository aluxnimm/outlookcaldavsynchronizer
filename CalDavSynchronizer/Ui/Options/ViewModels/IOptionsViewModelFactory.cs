using System;
using System.Collections.Generic;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  internal interface IOptionsViewModelFactory
  {
    List<OptionsViewModelBase> Create (ICollection<CalDavSynchronizer.Contracts.Options> options, bool fixInvalidSettings);
  }
}