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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using CalDavSynchronizer.Ui.Reports.ViewModels;

namespace CalDavSynchronizer.Ui.Reports.Views
{
    /// <summary>
    /// Interaction logic for ReportsView.xaml
    /// </summary>
    public partial class ReportsView : UserControl
    {
        private readonly ICollection<ReportViewModel> _selectedReports;

        public ReportsView()
        {
            InitializeComponent();
            DataContextChanged += ReportsView_DataContextChanged;
            _selectedReports = new ListToICollectionAdapter(ReportList.SelectedItems);
        }

        private void ReportsView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var newValue = (ReportsViewModel) e.NewValue;
            if (newValue != null)
            {
                ReportList.SelectedItems.Clear();

                if (!newValue.Reports.IsEmpty)
                    ReportList.SelectedItems.Add(newValue.Reports.Cast<ReportViewModel>().First());

                newValue.BindSelectedReports(_selectedReports);
            }

            var oldValue = (ReportsViewModel) e.OldValue;
            if (oldValue != null)
            {
                oldValue.BindSelectedReports(null);
            }
        }

        class ListToICollectionAdapter : ICollection<ReportViewModel>
        {
            private readonly IList _adapted;

            public ListToICollectionAdapter(IList adapted)
            {
                _adapted = adapted ?? throw new ArgumentNullException(nameof(adapted));
            }

            public IEnumerator<ReportViewModel> GetEnumerator()
            {
                return _adapted.Cast<ReportViewModel>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _adapted.GetEnumerator();
            }

            public void Add(ReportViewModel item)
            {
                _adapted.Add(item);
            }

            public void Clear()
            {
                _adapted.Clear();
            }

            public bool Contains(ReportViewModel item)
            {
                return _adapted.Contains(item);
            }

            public void CopyTo(ReportViewModel[] array, int arrayIndex)
            {
                _adapted.CopyTo(array, arrayIndex);
            }

            public bool Remove(ReportViewModel item)
            {
                _adapted.Remove(item);
                return true;
            }

            public int Count => _adapted.Count;
            public bool IsReadOnly => _adapted.IsReadOnly;
        }
    }
}