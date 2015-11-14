using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.Ui
{
  public partial class GenericConfigurationForm<TElement> : Form, IConfigurationForm<TElement>
  {
    private readonly XmlSerializer _serializer;

    public GenericConfigurationForm (object configurationData)
    {
      InitializeComponent();

      _serializer = new XmlSerializer (configurationData.GetType());
      var stringBuilder = new StringBuilder();

      using (var writer = new StringWriter (stringBuilder))
      {
        _serializer.Serialize (writer, configurationData);
      }

      _contentTextBox.Text = stringBuilder.ToString();
    }


    private void _okButton_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    public bool Display ()
    {
      return ShowDialog() == DialogResult.OK;
    }

    public TElement Options
    {
      get
      {
        using (var reader = new StringReader (_contentTextBox.Text))
        {
          return (TElement) _serializer.Deserialize (reader);
        }
      }
    }
  }
}