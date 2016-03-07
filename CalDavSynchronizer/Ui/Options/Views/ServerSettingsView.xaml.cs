using System;
using System.Windows.Controls;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.Ui.Options.Views
{
  /// <summary>
  ///   Interaction logic for ReportView.xaml
  /// </summary>
  public partial class ServerSettingsView : UserControl
  {
    private ServerSettingsViewModel _viewModel;

    public ServerSettingsView ()
    {
      InitializeComponent();
      DataContextChanged += ServerSettingsView_DataContextChanged;
      _passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
    }

    private void PasswordBox_PasswordChanged (object sender, System.Windows.RoutedEventArgs e)
    {
      if (_passwordBox.Password != _viewModel.Password)
        _viewModel.Password = _passwordBox.Password;
    }

    private void ServerSettingsView_DataContextChanged (object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
      _viewModel = e.NewValue as ServerSettingsViewModel;
      if (_viewModel != null)
      {
        _passwordBox.Password = _viewModel.Password;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
      }
    }

    private void ViewModel_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof (ServerSettingsViewModel.Password))
        _passwordBox.Password = _viewModel.Password;
    }
  }
}