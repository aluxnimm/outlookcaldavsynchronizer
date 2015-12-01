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