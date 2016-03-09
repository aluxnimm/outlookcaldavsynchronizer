using System;
using System.Windows.Controls;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Utilities;

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
      _viewModel.Password = _passwordBox.SecurePassword;
    }

    private void ServerSettingsView_DataContextChanged (object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
      _viewModel = e.NewValue as ServerSettingsViewModel;
      if (_viewModel != null)
      {
        // Password is just a OneWayBinding. Therefore just set the initial value
        _passwordBox.Password = SecureStringUtility.ToUnsecureString (_viewModel.Password);
      }
    }
  }
}