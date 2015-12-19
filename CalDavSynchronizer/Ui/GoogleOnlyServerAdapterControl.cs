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
using System.Reflection;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using log4net;

namespace CalDavSynchronizer.Ui
{
  public class GoogleOnlyServerAdapterControl : CheckBox, IServerAdapterControl
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    public event EventHandler SelectedServerAdapterTypeChanged;

    protected virtual void OnSelectedServerAdapterTypeChanged ()
    {
      var handler = SelectedServerAdapterTypeChanged;
      if (handler != null)
        handler (this, EventArgs.Empty);
    }

    public ServerAdapterType SelectedServerAdapterType
    {
      get { return Checked ? ServerAdapterType.GoogleOAuth : ServerAdapterType.Default; }
      set
      {
        switch (value)
        {
          case ServerAdapterType.GoogleOAuth:
            Checked = true;
            break;
          default:
            Checked = false;
            break;
        }
      }
    }

    public GoogleOnlyServerAdapterControl ()
    {
      CheckedChanged += GoogleOnlyServerAdapterControl_CheckedChanged;
      // ReSharper disable once DoNotCallOverridableMethodsInConstructor
      Text = "Use Google OAuth";
    }

    private void GoogleOnlyServerAdapterControl_CheckedChanged (object sender, EventArgs e)
    {
      OnSelectedServerAdapterTypeChanged();
    }

  }
}