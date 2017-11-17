using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.Ui.Options.ProfileTypes
{
  public class DesignProfileModelFactory : IProfileModelFactory
  {
    public static IProfileModelFactory Instance => new DesignProfileModelFactory();

    private DesignProfileModelFactory()
    {
    }

    public IProfileType ProfileType => DesignProfileType.Instance;

    public OptionsModel CreateNewModel()
    {
      return OptionsModel.DesignInstance;
    }

    public OptionsModel CreateModelFromData(Contracts.Options data)
    {
      return OptionsModel.DesignInstance;
    }

    public IOptionsViewModel CreateViewModel(OptionsModel model)
    {
      return GenericOptionsViewModel.DesignInstance;
    }

    public IOptionsViewModel CreateTemplateViewModel()
    {
      return GenericOptionsViewModel.DesignInstance;
    }

  }
}