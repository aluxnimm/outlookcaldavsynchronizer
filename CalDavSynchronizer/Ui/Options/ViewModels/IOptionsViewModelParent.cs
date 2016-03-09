using System;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public interface IOptionsViewModelParent
  {
    void RequestDeletion (OptionsViewModelBase viewModel);
    void RequestCopy (OptionsViewModelBase viewModel);
    void RequestCacheDeletion (OptionsViewModelBase viewModel);
  }
}
