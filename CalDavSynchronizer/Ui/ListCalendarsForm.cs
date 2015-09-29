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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CalDavSynchronizer.Ui
{
  public partial class ListCalendarsForm : Form
  {
    public ListCalendarsForm(IReadOnlyList<Tuple<Uri,string>> cals)
    {
      InitializeComponent();
      dataGridView1.DataSource = cals;
      dataGridView1.MultiSelect = false;
      dataGridView1.RowHeadersVisible = false;
      dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      dataGridView1.Columns[0].HeaderText = "Calendar Url";
      dataGridView1.Columns[1].HeaderText = "DisplayName";
    }

    private void ListCalendarsForm_Load(object sender, EventArgs e)
    {

    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      DialogResult = System.Windows.Forms.DialogResult.OK; 
    }

    public string getCalendarUri()
    {
      return dataGridView1.CurrentRow.Cells[0].Value.ToString();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = System.Windows.Forms.DialogResult.Cancel; 
    }
  }
}
