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
using System.Reflection;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using log4net;

namespace CalDavSynchronizer.Ui.Options
{
  public partial class FullServerdApterTypeControl : UserControl, IServerAdapterControl
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly IList<Item<ServerAdapterType>> _availableServerAdapterTypes = new List<Item<ServerAdapterType>>()
                                                                                   {
                                                                                       new Item<ServerAdapterType> (ServerAdapterType.WebDavHttpClientBased, "WebDav"),
                                                                                       new Item<ServerAdapterType> (ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth, "WebDav with GoogleOAuth"),
                                                                                       new Item<ServerAdapterType> (ServerAdapterType.WebDavSynchronousWebRequestBased, "WebDav (Synchronous)"),
                                                                                       new Item<ServerAdapterType> (ServerAdapterType.GoogleTaskApi, "Google Task-API"),
                                                                                   };

    public event EventHandler SelectedServerAdapterTypeChanged;

    protected virtual void OnSelectedServerAdapterTypeChanged ()
    {
      var handler = SelectedServerAdapterTypeChanged;
      if (handler != null)
        handler (this, EventArgs.Empty);
    }

    public ServerAdapterType SelectedServerAdapterType
    {
      get { return (ServerAdapterType) _serverAdapterTypeComboBox.SelectedValue; }
      set { _serverAdapterTypeComboBox.SelectedValue = value; }
    }

    private void ServerAdapterTypeComboBox_SelectedValueChanged (object sender, EventArgs e)
    {
      OnSelectedServerAdapterTypeChanged();
    }

    public FullServerdApterTypeControl ()
    {
      InitializeComponent();
      Item.BindComboBox (_serverAdapterTypeComboBox, _availableServerAdapterTypes);
      _serverAdapterTypeComboBox.SelectedValueChanged += ServerAdapterTypeComboBox_SelectedValueChanged;
    }
  }
}