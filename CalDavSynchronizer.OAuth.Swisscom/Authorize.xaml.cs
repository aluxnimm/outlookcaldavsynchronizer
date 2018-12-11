using DotNetOpenAuth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CalDavSynchronizer.OAuth.Swisscom
{
    /// <summary>
    /// Interaction logic for Authorize.xaml
    /// </summary>
    public partial class Authorize : Window
    {
        public Authorize(UserAgentClient client)
        {
            InitializeComponent();
            clientAuthorizationView.Client = client;
        }
        public IAuthorizationState Authorization
        {
            get { return this.clientAuthorizationView.Authorization; }
        }

        private void clientAuthorizationView_Completed(object sender, ClientAuthorizationCompleteEventArgs e)
        {
            this.DialogResult = e.Authorization != null;
            this.Close();
        }
    }
}

