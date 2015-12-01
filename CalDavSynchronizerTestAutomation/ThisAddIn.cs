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
using Microsoft.Office.Tools.Ribbon;
using Office = Microsoft.Office.Core;

namespace CalDavSynchronizerTestAutomation
{
  public partial class ThisAddIn
  {
    private void ThisAddIn_Startup (object sender, System.EventArgs e)
    {
    }

    private void ThisAddIn_Shutdown (object sender, System.EventArgs e)
    {
    }


    protected override IRibbonExtension[] CreateRibbonObjects ()
    {
      return new IRibbonExtension[] { new CalDavSynchronizerTestRibbon() };
    }

    #region VSTO generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InternalStartup ()
    {
      this.Startup += new System.EventHandler (ThisAddIn_Startup);
      this.Shutdown += new System.EventHandler (ThisAddIn_Shutdown);
    }

    #endregion
  }
}