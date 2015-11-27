// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using System.Windows.Forms;
using CalDavSynchronizer.Ui.ConnectionTests;
using System.Drawing;
using CalDavSynchronizer.Utilities;

namespace CalDavSynchronizer.Ui
{
  public partial class SelectResourceForm : Form
  {
    public string SelectedUrl { get; private set; }
    public ResourceType ResourceType { get; private set; }

    public SelectResourceForm (IReadOnlyList<Tuple<Uri, string, string>> caldendars, IReadOnlyList<Tuple<Uri, string>> addressBooks, bool displayAddressBooksInitial)
    {
      InitializeComponent();
      _calendarDataGridView.DataSource = caldendars;
      _calendarDataGridView.Columns[0].HeaderText = "CalDav Url";
      _calendarDataGridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      _calendarDataGridView.Columns[1].HeaderText = "DisplayName";
      _calendarDataGridView.Columns[2].HeaderText = "Col";
      _calendarDataGridView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;

      _addressBookDataGridView.DataSource = addressBooks;
      _addressBookDataGridView.Columns[0].HeaderText = "CardDav Url";
      _addressBookDataGridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      _addressBookDataGridView.Columns[1].HeaderText = "DisplayName";

      _mainTab.SelectedTab = displayAddressBooksInitial ? _addressBookPage : _calendarPage;
    }

    private void buttonCancel_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
    }

    private void _calendarDataGridView_CellContentDoubleClick (object sender, DataGridViewCellEventArgs e)
    {
      if (e.RowIndex >= 0)
      {
        SelectedUrl = _calendarDataGridView.Rows[e.RowIndex].Cells[0].Value.ToString();
        ResourceType = ResourceType.Calendar;
        DialogResult = DialogResult.OK;
      }
    }

    private void _addressBookDataGridView_CellContentDoubleClick (object sender, DataGridViewCellEventArgs e)
    {
      if (e.RowIndex >= 0)
      {
        SelectedUrl = _addressBookDataGridView.Rows[e.RowIndex].Cells[0].Value.ToString();
        ResourceType = ResourceType.AddressBook;
        DialogResult = DialogResult.OK;
      }
    }

    private void _calendarDataGridView_CellFormatting (object sender, DataGridViewCellFormattingEventArgs e)
    {
      if (!_calendarDataGridView.Rows[e.RowIndex].IsNewRow)
      {
        if (e.ColumnIndex == 2)
        {
          if (!string.IsNullOrEmpty (e.Value.ToString()))
          {
            Color calColor = ColorHelper.HexToColor (e.Value.ToString());
            e.CellStyle.ForeColor = calColor;
            e.CellStyle.BackColor = calColor;
            e.CellStyle.SelectionBackColor = calColor;
            e.CellStyle.SelectionForeColor = calColor;
          }
        }
      }
    }

    private void OkButton_Click (object sender, EventArgs e)
    {

      var visibleGrid = _mainTab.SelectedTab == _calendarPage ? _calendarDataGridView : _addressBookDataGridView;

      if (visibleGrid.SelectedRows.Count == 0)
      {
        MessageBox.Show ("No ressource selected!");
      }
      else
      {
        ResourceType = (visibleGrid == _calendarDataGridView) ? ResourceType.Calendar : ResourceType.AddressBook;
        SelectedUrl = visibleGrid.SelectedRows[0].Cells[0].Value.ToString();
        DialogResult = DialogResult.OK;
      }
    }
  }
}