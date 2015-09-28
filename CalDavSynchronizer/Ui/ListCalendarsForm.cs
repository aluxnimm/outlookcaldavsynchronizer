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
      dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
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
