using log4net;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CalDavSynchronizer.OAuth.Swisscom
{
    /// <summary>
    /// Interaction logic for <see cref="LocationSelectorWindow"/>.xaml
    /// </summary>
    public partial class LocationSelectorWindow : Window
    {
        private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private AddressbookInfo location;
        internal LocationSelectorWindow(AddressbookInfo[] locations)
        {
            InitializeComponent();
            foreach (AddressbookInfo l in locations)
            {
                String content = l.LocationInfo.AddressLine1;
                String toolTip = l.LocationInfo.AddressLine1;
                if (l.ListOfMsisdn.Length > 0)
                {
                    if (String.IsNullOrEmpty(content))
                    {
                        content = l.ListOfMsisdn[0];
                        toolTip = String.Join(",", l.ListOfMsisdn);
                    }
                    else
                    {
                        content = content + " (" + l.ListOfMsisdn[0] + ")";
                        toolTip = toolTip + "\n(" + String.Join(",", l.ListOfMsisdn) + ")";
                    }
                }
                RadioButton radioButton = new RadioButton()
                {
                    GroupName = Globalization.Strings.Localize("Location"),
                    Content = content,
                    DataContext = l,
                    Height = 25,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    ToolTip = toolTip,
                };
                radioButton.Click += new RoutedEventHandler(RadioButtom_Click);

                LocationListBox.Items.Add(radioButton);
            }
        }

        private void RadioButtom_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("RadioButtom_Click: " + sender.ToString());
            //System.Diagnostics.Debug.WriteLine("RadioButton_Click: " + sender.ToString());
            //s_logger.Debug("RadioButton_Click: " + sender.ToString());
            if ((bool)((RadioButton)sender).IsChecked)
            {
                location = (AddressbookInfo)((RadioButton)sender).DataContext;
                Button_Ok.IsEnabled = true;
            }
        }

        private void Action_LocationSelectorWindow_Cancel(object sender, CancelEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Action_LocationSelectorWindow_Cancel: " + sender.ToString());
        }

        private void Action_LocationSelectorWindow_Button_Ok(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Action_LocationSelectorWindow_Button_Ok: " + sender.ToString());

            if (location != null)
            {
                Close();
            }
        }
        private void Action_LocationSelectorWindow_Button_Cancel(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Action_LocationSelectorWindow_Button_Cancel: " + sender.ToString());
            location = null;
            Close();
        }
        public AddressbookInfo GetSelectedLocationInfo()
        {
            return location;
        }
    }
}
