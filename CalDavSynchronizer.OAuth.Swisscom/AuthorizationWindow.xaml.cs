using DotNetOpenAuth.OAuth2;
using System.Linq;
using System.Windows;

namespace CalDavSynchronizer.OAuth.Swisscom
{
    /// <summary>
    /// Interaction logic for AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow(UserAgentClient client)
        {
            InitializeComponent();

            var clientAuthorizationViewWebBrowserOrNull = clientAuthorizationView.Controls.OfType<System.Windows.Forms.WebBrowser>().FirstOrDefault();
            if (clientAuthorizationViewWebBrowserOrNull != null) {
                clientAuthorizationViewWebBrowserOrNull.ScriptErrorsSuppressed = true;
            }
            clientAuthorizationView.Client = client;
        }
        public IAuthorizationState Authorization
        {
            get { return this.clientAuthorizationView.Authorization; }
        }

        private void ClientAuthorizationView_Completed(object sender, ClientAuthorizationCompleteEventArgs e)
        {
            this.DialogResult = e.Authorization != null;
            this.Close();
        }
    }
}

