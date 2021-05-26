using CalDavSynchronizer.OAuth.Daimler.Models;
using CalDavSynchronizer.ProfileTypes.ConcreteTypes.Daimler.ViewModels;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CalDavSynchronizer.Ui.Options.Views
{
    /// <summary>
    /// Interaction logic for DaimlerSettingsView.xaml
    /// </summary>
    public partial class DaimlerSettingsView : System.Windows.Controls.UserControl
    {
        public DaimlerSettingsView()
        {
            InitializeComponent();
        }

        private void btnSelectConfig_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Config (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var jsonContent = File.ReadAllText(openFileDialog.FileName);
                    var json = JsonConvert.DeserializeObject<DaimlerOptions>(jsonContent);
                    if (json.Environments.Length == 0)
                    {
                        MessageBox.Show(
                            text: "No data found in the configuration file",
                            caption: "No data",
                            buttons: MessageBoxButtons.OK,
                            icon: MessageBoxIcon.Warning);
                        return;
                    }

                    (DataContext as DaimlerServerSettingsViewModel).DaimlerConfig = json;
                }
                catch (System.Exception)
                {
                    MessageBox.Show(
                        text: "Not valid config file",
                        caption: "Error",
                        buttons: MessageBoxButtons.OK,
                        icon: MessageBoxIcon.Error);
                }
            }
        }
    }
}