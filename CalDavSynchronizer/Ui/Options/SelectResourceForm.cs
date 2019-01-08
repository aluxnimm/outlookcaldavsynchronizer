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
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Ui.ConnectionTests;
using CalDavSynchronizer.Ui.Options.ResourceSelection.ViewModels;
using CalDavSynchronizer.Utilities;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options
{
  public partial class SelectResourceForm : Form
  {
    public object SelectedObject { get; private set; }
    public ResourceType ResourceType { get; private set; }

    public static SelectResourceForm CreateForResourceSelection (
     ResourceType initialResourceTabToDisplay,
     IReadOnlyList<CalendarDataViewModel> calendars = null,
     IReadOnlyList<AddressBookDataViewModel> addressBooks = null,
     IReadOnlyList<TaskListDataViewModel> taskLists = null)
    {
      return new SelectResourceForm (initialResourceTabToDisplay, calendars, addressBooks, taskLists);
    }

    public static SelectResourceForm CreateForFolderAssignment(
      IOptionTasks optionTasks,
      ResourceType initialResourceTabToDisplay,
      IReadOnlyList<CalendarDataViewModel> calendars = null,
      IReadOnlyList<AddressBookDataViewModel> addressBooks = null,
      IReadOnlyList<TaskListDataViewModel> taskLists = null)
    {
      return new SelectResourceForm(initialResourceTabToDisplay, optionTasks, calendars, addressBooks, taskLists);
    }

    private SelectResourceForm (
      ResourceType initialResourceTabToDisplay,
      IReadOnlyList<CalendarDataViewModel> calendars = null, 
      IReadOnlyList<AddressBookDataViewModel> addressBooks = null, 
      IReadOnlyList<TaskListDataViewModel> taskLists = null)
    {
      InitializeComponent();

      Text = Strings.Get($"Select Resource");
      OkButton.Text = Strings.Get($"OK");
      buttonCancel.Text = Strings.Get($"Cancel");
      _calendarPage.Text = Strings.Get($"Calendars");
      _addressBookPage.Text = Strings.Get($"Address Books");
      _tasksPage.Text = Strings.Get($"Tasks");

      if (calendars != null)
      {
        _calendarDataGridView.DataSource = calendars;

        // ReSharper disable PossibleNullReferenceException
        _calendarDataGridView.Columns[nameof (CalendarDataViewModel.Uri)].Visible = false;
        _calendarDataGridView.Columns[nameof (CalendarDataViewModel.Name)].HeaderText = Strings.Get($"Name");
        _calendarDataGridView.Columns[nameof (CalendarDataViewModel.Name)].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        _calendarDataGridView.Columns[nameof (CalendarDataViewModel.Color)].HeaderText = Strings.Get($"Col");
        _calendarDataGridView.Columns[nameof (CalendarDataViewModel.Color)].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        _calendarDataGridView.Columns[nameof (CalendarDataViewModel.Privileges)].HeaderText = Strings.Get($"Access");
        _calendarDataGridView.Columns[nameof (CalendarDataViewModel.Privileges)].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        _calendarDataGridView.Columns[nameof (CalendarDataViewModel.Privileges)].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _calendarDataGridView.Columns[nameof (CalendarDataViewModel.SelectedFolder)].Visible = false;
        _calendarDataGridView.Columns[nameof (CalendarDataViewModel.Model)].Visible = false;


        // ReSharper restore PossibleNullReferenceException
      }
      else
      {
        _mainTab.TabPages.Remove (_calendarPage);
      }

      if (addressBooks != null)
      {
        // ReSharper disable PossibleNullReferenceException
        _addressBookDataGridView.DataSource = addressBooks;
        _addressBookDataGridView.Columns[nameof (AddressBookDataViewModel.Uri)].Visible = false;
        _addressBookDataGridView.Columns[nameof (AddressBookDataViewModel.Name)].HeaderText = Strings.Get($"Name");
        _addressBookDataGridView.Columns[nameof (AddressBookDataViewModel.Name)].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        _addressBookDataGridView.Columns[nameof (AddressBookDataViewModel.Privileges)].HeaderText = Strings.Get($"Access");
        _addressBookDataGridView.Columns[nameof (AddressBookDataViewModel.Privileges)].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        _addressBookDataGridView.Columns[nameof (AddressBookDataViewModel.Privileges)].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _addressBookDataGridView.Columns[nameof (AddressBookDataViewModel.SelectedFolder)].Visible = false;
        _addressBookDataGridView.Columns[nameof (AddressBookDataViewModel.Model)].Visible = false;
        _addressBookDataGridView.CellFormatting += _addressBookDataGridView_CellFormatting;
        // ReSharper restore PossibleNullReferenceException
      }
      else
      {
        _mainTab.TabPages.Remove (_addressBookPage);
      }

      if (taskLists != null)
      {
        // ReSharper disable PossibleNullReferenceException
        _tasksDataGridView.DataSource = taskLists;
        _tasksDataGridView.Columns[nameof (TaskListDataViewModel.Id)].Visible = false;
        _tasksDataGridView.Columns[nameof (TaskListDataViewModel.Name)].HeaderText = Strings.Get($"Name");
        _tasksDataGridView.Columns[nameof (TaskListDataViewModel.Name)].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        _tasksDataGridView.Columns[nameof (TaskListDataViewModel.Privileges)].HeaderText = Strings.Get($"Access");
        _tasksDataGridView.Columns[nameof (TaskListDataViewModel.Privileges)].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        _tasksDataGridView.Columns[nameof (TaskListDataViewModel.Privileges)].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _tasksDataGridView.Columns[nameof (TaskListDataViewModel.SelectedFolder)].Visible = false;
        _tasksDataGridView.Columns[nameof (TaskListDataViewModel.Model)].Visible = false;
        _tasksDataGridView.CellFormatting += _tasksDataGridView_CellFormatting;

        // ReSharper restore PossibleNullReferenceException
      }
      else
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

    public sealed override string Text
    {
      get => base.Text;
      set => base.Text = value;
    }

    private SelectResourceForm (
        ResourceType initialResourceTabToDisplay,
        IOptionTasks optionTasks,
        IReadOnlyList<CalendarDataViewModel> calendars = null,
        IReadOnlyList<AddressBookDataViewModel> addressBooks = null,
        IReadOnlyList<TaskListDataViewModel> taskLists = null)
        : this (initialResourceTabToDisplay, calendars, addressBooks, taskLists)
    {
      if (calendars != null)
        SetupFolderSelectionColumns (_calendarDataGridView, optionTasks, OlItemType.olAppointmentItem);

      if (addressBooks != null)
        SetupFolderSelectionColumns (_addressBookDataGridView, optionTasks, OlItemType.olContactItem);

      if (taskLists != null)
        SetupFolderSelectionColumns (_tasksDataGridView, optionTasks, OlItemType.olTaskItem);
    }

    private static void SetupFolderSelectionColumns (DataGridView dataGridView, IOptionTasks optionTasks, params OlItemType[] allowedFolderType)
    {
      var folderColumn = dataGridView.Columns[nameof(ResourceDataViewModelBase.SelectedFolder)];
      folderColumn.Visible = true;
      folderColumn.HeaderText = Strings.Get($"Selected Outlook Folder");
      folderColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

      var selectFolderColumn = new DataGridViewButtonColumn();
      selectFolderColumn.UseColumnTextForButtonValue = true;
      selectFolderColumn.Text = "...";
      selectFolderColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridView.Columns.Add (selectFolderColumn);

      var removeFolderColumn = new DataGridViewButtonColumn();
      removeFolderColumn.UseColumnTextForButtonValue = true;
      removeFolderColumn.Text = "x";
      removeFolderColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGridView.Columns.Add (removeFolderColumn);

      dataGridView.CellContentClick += (sender, e) =>
      {
        var column = dataGridView.Columns[e.ColumnIndex];
        var row = dataGridView.Rows[e.RowIndex];
        var viewModel = (ResourceDataViewModelBase) row.DataBoundItem;

        if (column == selectFolderColumn)
        {
          var folder = optionTasks.PickFolderOrNull();
          if (folder != null)
          {
            if (Array.IndexOf (allowedFolderType, folder.DefaultItemType) == -1)
            {
              MessageBox.Show (Strings.Get($"Folder has to have item type '{String.Join (", ", allowedFolderType)}'."), Strings.Get($"Select folder"), MessageBoxButtons.OK, MessageBoxIcon.Error);
              return;
            }

            viewModel.SelectedFolder = folder;
            dataGridView.UpdateCellValue(dataGridView.Columns[nameof(ResourceDataViewModelBase.SelectedFolder)].Index, row.Index);
          }
        }
        else if (column == removeFolderColumn)
        {
          viewModel.SelectedFolder = null;
          dataGridView.UpdateCellValue(dataGridView.Columns[nameof(ResourceDataViewModelBase.SelectedFolder)].Index, row.Index);
        }
      };
    }

    private void buttonCancel_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
    }

    private void _calendarDataGridView_CellContentDoubleClick (object sender, DataGridViewCellEventArgs e)
    {
      if (e.RowIndex >= 0)
      {
        SelectedObject = _calendarDataGridView.Rows[e.RowIndex].DataBoundItem;
        ResourceType = ResourceType.Calendar;
        DialogResult = DialogResult.OK;
      }
    }

    private void _addressBookDataGridView_CellContentDoubleClick (object sender, DataGridViewCellEventArgs e)
    {
      if (e.RowIndex >= 0)
      {
        SelectedObject = _addressBookDataGridView.Rows[e.RowIndex].DataBoundItem;
        ResourceType = ResourceType.AddressBook;
        DialogResult = DialogResult.OK;
      }
    }

    private void _calendarDataGridView_CellFormatting (object sender, DataGridViewCellFormattingEventArgs e)
    {
      if (!_calendarDataGridView.Rows[e.RowIndex].IsNewRow)
      {
        var columnName = _calendarDataGridView.Columns[e.ColumnIndex].Name;
        var cell = _calendarDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
        if (columnName == nameof(CalendarDataViewModel.Color))
        {
          if (e.Value != null)
          {
            Color calColor = Color.FromArgb(((ArgbColor)e.Value).ArgbValue);
            e.CellStyle.ForeColor = calColor;
            e.CellStyle.BackColor = calColor;
            e.CellStyle.SelectionBackColor = calColor;
            e.CellStyle.SelectionForeColor = calColor;
            cell.ToolTipText = calColor.ToString();
          }
        }
        else if (columnName == nameof(CalendarDataViewModel.Name))
        {
          cell.ToolTipText = (_calendarDataGridView.Rows[e.RowIndex].Cells[nameof(CalendarDataViewModel.Uri)].Value as Uri)?.AbsolutePath;
        }
        else if (columnName == nameof (CalendarDataViewModel.Uri))
        {
          e.Value = (e.Value as Uri)?.AbsolutePath;
        }
        else if (columnName == nameof(ResourceDataViewModelBase.SelectedFolder))
        {
          e.Value = (e.Value as OutlookFolderDescriptor)?.Name;
        }
      }
    }

    private void _addressBookDataGridView_CellFormatting (object sender, DataGridViewCellFormattingEventArgs e)
    {
      var columnName = _addressBookDataGridView.Columns[e.ColumnIndex].Name;
      if (columnName == nameof (AddressBookDataViewModel.Uri))
      {
        e.Value = (e.Value as Uri)?.AbsolutePath;
      }
      else if (columnName == nameof(AddressBookDataViewModel.Name))
      {
        var cell = _addressBookDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
        cell.ToolTipText = (_addressBookDataGridView.Rows[e.RowIndex].Cells[nameof(AddressBookDataViewModel.Uri)].Value as Uri)?.AbsolutePath;
      }
      else if (columnName == nameof(ResourceDataViewModelBase.SelectedFolder))
      {
        e.Value = (e.Value as OutlookFolderDescriptor)?.Name;
      }
    }

    private void _tasksDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      var columnName = _tasksDataGridView.Columns[e.ColumnIndex].Name;
      if (columnName == nameof(ResourceDataViewModelBase.SelectedFolder))
      {
        e.Value = (e.Value as OutlookFolderDescriptor)?.Name;
      }
      else if (columnName == nameof(TaskListDataViewModel.Name))
      {
        var cell = _tasksDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
        cell.ToolTipText = _tasksDataGridView.Rows[e.RowIndex].Cells[nameof(TaskListDataViewModel.Id)].Value?.ToString();
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
        MessageBox.Show (Strings.Get($"No resource selected!"));
      }
      else
      {
        SelectedObject = visibleGrid.SelectedRows[0].DataBoundItem;
        DialogResult = DialogResult.OK;
      }
    }

    private void _tasksDataGridView_CellContentDoubleClick (object sender, DataGridViewCellEventArgs e)
    {
      if (e.RowIndex >= 0)
      {
        SelectedObject = _tasksDataGridView.Rows[e.RowIndex].DataBoundItem;
        ResourceType = ResourceType.TaskList;
        DialogResult = DialogResult.OK;
      }
    }
  }
}