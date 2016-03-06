using System;
using System.Windows;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.Ui.Options.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class OptionsWindow : Window
  {
    public OptionsWindow ()
    {
      InitializeComponent ();
      this.DataContextChanged += OptionsWindow_DataContextChanged;
    }

    private void OptionsWindow_DataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
    {
      var viewModel = e.NewValue as OptionsCollectionViewModel;
      if (viewModel != null)
      {
        viewModel.CloseRequested += ViewModel_CloseRequested;
      }
    }

    private void ViewModel_CloseRequested (object sender, CloseEventArgs e)
    {
      DialogResult = e.ShouldSaveNewOptions;
    }
  }
}
