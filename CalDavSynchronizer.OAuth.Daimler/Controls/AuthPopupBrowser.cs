using IdentityModel.OidcClient.Browser;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CalDavSynchronizer.OAuth.Daimler.Controls
{
    public class AuthPopupBrowser : IBrowser
    {
        readonly string _title;

        public AuthPopupBrowser()
            : this("Daimler Login")
        {
        }

        public AuthPopupBrowser(string title)
        {
            _title = title;
        }

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken)
        {
            var result = new BrowserResult() { ResultType = BrowserResultType.UserCancel };
            var doneThread = new ManualResetEvent(false);

            var host = new Window()
            {
                Width = 480,
                Height = 570,
                Title = _title,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var webBrowser = new WebBrowser { AllowDrop = false };
            host.Content = webBrowser;

            webBrowser.Navigating += (s, e) =>
            {
                if (e.Uri.AbsoluteUri.StartsWith(options.EndUrl))
                {
                    e.Cancel = true;

                    result = new BrowserResult()
                    {
                        ResultType = BrowserResultType.Success,
                        Response = e.Uri.AbsoluteUri
                    };

                    host.Close();

                        //doneThread[0].Set();
                    }
            };

            host.Closing += (s, e) =>
            {
                doneThread.Set();
            };

            host.Show();
            webBrowser.Navigate(new Uri(options.StartUrl));

            await Task.Factory.StartNew(() => doneThread.WaitOne());

            return result;
        }
    }
}
