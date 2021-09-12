// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CalDavSynchronizer.Utilities;
using Microsoft.Office.Interop.Outlook;
using System.Linq;
using CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.Ui.Options.Views
{
    public class ToProfileImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var isMultipleOptionsTemplateViewModel = (bool?) values[1];

            if (isMultipleOptionsTemplateViewModel.GetValueOrDefault(false))
                return BitmapFrame.Create(new Uri("pack://application:,,,/CalDavSynchronizer;component/Resources/AddMultiple.png"));

            var itemType = (OlItemType?) values[0];
            switch (itemType)
            {
                case OlItemType.olAppointmentItem:
                    return BitmapFrame.Create(new Uri("pack://application:,,,/CalDavSynchronizer;component/Resources/Appointment.png"));
                case OlItemType.olTaskItem:
                    return BitmapFrame.Create(new Uri("pack://application:,,,/CalDavSynchronizer;component/Resources/Task.png"));
                case OlItemType.olContactItem:
                    return BitmapFrame.Create(new Uri("pack://application:,,,/CalDavSynchronizer;component/Resources/Contact.png"));
            }

            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new[] {Binding.DoNothing};
        }
    }
}