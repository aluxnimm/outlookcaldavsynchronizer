using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.Ui.Options.ViewModels.Mapping
{
  public class ContactMappingConfigurationViewModel : INotifyPropertyChanged, IOptionsViewModel
  {
    private bool _mapBirthday;
    private bool _mapContactPhoto;

    public bool MapBirthday
    {
      get { return _mapBirthday; }
      set
      {
        _mapBirthday = value;
        OnPropertyChanged();
      }
    }

    public bool MapContactPhoto
    {
      get { return _mapContactPhoto; }
      set
      {
        _mapContactPhoto = value;
        OnPropertyChanged();
      }
    }

    public static ContactMappingConfigurationViewModel DesignInstance => new ContactMappingConfigurationViewModel
                                                                         {
                                                                             MapBirthday = true,
                                                                             MapContactPhoto = true
                                                                         };

    public event PropertyChangedEventHandler PropertyChanged;


    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      SetOptions(options.MappingConfiguration as ContactMappingConfiguration ?? new ContactMappingConfiguration());
    }

    public void SetOptions (ContactMappingConfiguration mappingConfiguration)
    {
      MapBirthday = mappingConfiguration.MapBirthday;
      MapContactPhoto = mappingConfiguration.MapContactPhoto;
    }

    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      options.MappingConfiguration = new ContactMappingConfiguration
                                     {
                                         MapBirthday = _mapBirthday,
                                         MapContactPhoto = _mapContactPhoto
                                     };
    }

    public string Name => "Contact mapping configuration";

    public bool Validate (StringBuilder errorBuilder)
    {
      return true;
    }

    public IEnumerable<IOptionsViewModel> SubOptions => new IOptionsViewModel[] { };

    protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
    }
  }
}