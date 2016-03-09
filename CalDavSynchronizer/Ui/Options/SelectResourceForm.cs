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
using System.Drawing;
using System.Windows.Forms;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Ui.ConnectionTests;
using CalDavSynchronizer.Utilities;

namespace CalDavSynchronizer.Ui.Options
{
  public partial class SelectResourceForm : Form
  {
    public string SelectedUrl { get; private set; }
    public ResourceType ResourceType { get; private set; }

    public SelectResourceForm (
      IReadOnlyList<Tuple<Uri, string, ArgbColor?>> caldendars, 
      IReadOnlyList<Tuple<Uri, string>> addressBooks, 
      IReadOnlyList<Tuple<string, string>> taskLists,
      ResourceType initialResourceTabToDisplay)
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

      if (taskLists.Count > 0)
      {
        _tasksDataGridView.DataSource = taskLists;
        _tasksDataGridView.Columns[0].HeaderText = "Task List Id";
        _tasksDataGridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        _tasksDataGridView.Columns[0].Visible = false;
        _tasksDataGridView.Columns[1].HeaderText = "Task List";
      }
      else if (initialResourceTabToDisplay != ResourceType.TaskList)
      {
        _mainTab.TabPages.Remove (_tasksPage);
      }

      switch (initialResourceTabToDisplay)
      {
        case ResourceType.None:
        case ResourceType.AddressBook:
          _mainTab.SelectedTab = _addressBookPage;
          break;
        case ResourceType.Calendar:
          _mainTab.SelectedTab = _calendarPage;
          break;
        case ResourceType.TaskList:
          _mainTab.SelectedTab = _tasksPage;
          break;
      }
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
          if (e.Value != null)
          {
            Color calColor = Color.FromArgb(((ArgbColor)e.Value).ArgbValue);
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
      DataGridView visibleGrid = null;

      if (_mainTab.SelectedTab == _calendarPage)
      {
        visibleGrid = _calendarDataGridView;
        ResourceType = ResourceType.Calendar;
      }
      else if (_mainTab.SelectedTab == _addressBookPage)
      {
        visibleGrid = _addressBookDataGridView;
        ResourceType = ResourceType.AddressBook;
      }
      if (_mainTab.SelectedTab == _tasksPage)
      {
        visibleGrid = _tasksDataGridView;
        ResourceType = ResourceType.TaskList;
      }

      if (visibleGrid == null)
        throw new NotImplementedException();

      if (visibleGrid.SelectedRows.Count == 0)
      {
        ResourceType = ResourceType.None;
        MessageBox.Show ("No ressource selected!");
      }
      else
      {
        SelectedUrl = visibleGrid.SelectedRows[0].Cells[0].Value.ToString();
        DialogResult = DialogResult.OK;
      }
    }

    private void _tasksDataGridView_CellContentDoubleClick (object sender, DataGridViewCellEventArgs e)
    {
      if (e.RowIndex >= 0)
      {
        SelectedUrl = _tasksDataGridView.Rows[e.RowIndex].Cells[0].Value.ToString ();
        ResourceType = ResourceType.TaskList;
        DialogResult = DialogResult.OK;
      }
    }
  }
}