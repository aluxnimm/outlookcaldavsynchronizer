using log4net;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;

namespace CalDavSynchronizer.OAuth.Swisscom
{
    /// <summary>
    /// Interaction logic for <see cref="ErrorWindow"/>.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Hyperlink customerCenterLink;

        public ErrorWindow(string errorMessage)
        {
            customerCenterLink = new Hyperlink() { FontWeight = FontWeights.Bold };
            customerCenterLink.Inlines.Add(Globalization.Strings.Localize("LABEL_HELP_LINK_NAME"));
            customerCenterLink.NavigateUri = new Uri(Globalization.Strings.Localize("LABEL_HELP_LINK_URL"));
            customerCenterLink.Click += new RoutedEventHandler(Action_LinkClicked);

            InitializeComponent();
            TextBlock ErrorMessageTextBlock = new TextBlock();
            ErrorMessageTextBlock.TextWrapping = TextWrapping.Wrap;
            ErrorMessageTextBlock.Margin = new Thickness(5);

            ErrorMessageTextBlock.Inlines.Add(new Run(errorMessage));
            ErrorMessageTextBlock.Inlines.Add(new LineBreak());
            ErrorMessageTextBlock.Inlines.Add(new LineBreak());
            ErrorMessageTextBlock.Inlines.Add(new Run(Globalization.Strings.Localize("LABEL_MORE_INFO")));
            ErrorMessageTextBlock.Inlines.Add(new LineBreak());
            ErrorMessageTextBlock.Inlines.Add(this.customerCenterLink);
            ErrorMessageTextBlock.Inlines.Add(new LineBreak());

            this.ErrorDockPanel.Children.Add(ErrorMessageTextBlock);
        }

        private void Action_ErrorWindow_Cancel(object sender, CancelEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Action_ErrorWindow_Cancel: " + sender.ToString());
        }

        private void Action_ErrorWindow_Button_Close(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Action_ErrorWindow_Button_Close: " + sender.ToString());
            Close();
        }

        private void Action_LinkClicked(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Action_LinkClicked: " + sender.ToString());
            Process.Start(customerCenterLink.NavigateUri.ToString());
        }
    }
}
