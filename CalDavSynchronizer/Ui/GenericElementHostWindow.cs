using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace CalDavSynchronizer.Ui
{
  public partial class GenericElementHostWindow : Form
  {
    public GenericElementHostWindow ()
    {
      InitializeComponent();
    }

    public UIElement Child
    {
      get { return _elementHost.Child; }
      set { _elementHost.Child = value; }
    }
  }
}