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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CalDavSynchronizer.Ui.ViewModels
{
    public class GenericReportViewModel : ModelBase
    {
        private string _reportText;
        private string _title;

        public event EventHandler CloseRequested;

        public GenericReportViewModel()
        {
            OkCommand = new DelegateCommand(_ => OnCloseRequested());
        }

        public ICommand OkCommand { get; }

        public string ReportText
        {
            get { return _reportText; }
            set { CheckedPropertyChange(ref _reportText, value); }
        }

        private void OnCloseRequested()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        public static GenericReportViewModel DesingInstance => new GenericReportViewModel() {Title = "The title", ReportText = "This is an important report"};

        public string Title
        {
            get { return _title; }
            set { CheckedPropertyChange(ref _title, value); }
        }

        public void AppendLine(string text)
        {
            ReportText = ReportText + Environment.NewLine + text;
        }
    }
}