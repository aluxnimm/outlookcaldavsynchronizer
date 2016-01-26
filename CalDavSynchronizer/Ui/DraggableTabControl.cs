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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.using System;using System;

using System.Windows.Forms;

namespace CalDavSynchronizer.Ui
{
  public class DraggableTabControl : TabControl
  {
    private TabPage _draggedTab;

    public DraggableTabControl ()
    {
      AllowDrop = true;
    }

    protected override void OnMouseDown (MouseEventArgs e)
    {
      _draggedTab = GetPointedTab();

      base.OnMouseDown (e);
    }

    protected override void OnMouseUp (MouseEventArgs e)
    {
      _draggedTab = null;

      base.OnMouseUp (e);
    }

    protected override void OnMouseMove (MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left && _draggedTab != null)
        DoDragDrop (_draggedTab, DragDropEffects.Move);

      base.OnMouseMove (e);
    }

    protected override void OnDragOver (DragEventArgs drgevent)
    {
      TabPage draggedTab = (TabPage) drgevent.Data.GetData (typeof (TabPage));
      TabPage pointedTab = GetPointedTab();

      if (draggedTab == _draggedTab && pointedTab != null)
      {
        drgevent.Effect = DragDropEffects.Move;

        if (pointedTab != draggedTab)
          SwapTabPages (draggedTab, pointedTab);
      }

      base.OnDragOver (drgevent);
    }

    private TabPage GetPointedTab ()
    {
      for (int i = 0; i < TabPages.Count; i++)
        if (GetTabRect (i).Contains (PointToClient (Cursor.Position)))
          return TabPages[i];

      return null;
    }

    private void SwapTabPages (TabPage src, TabPage dst)
    {
      int srci = TabPages.IndexOf (src);
      int dsti = TabPages.IndexOf (dst);

      TabPages[dsti] = src;
      TabPages[srci] = dst;

      if (SelectedIndex == srci)
        SelectedIndex = dsti;
      else if (SelectedIndex == dsti)
        SelectedIndex = srci;

      Refresh();
    }
  }
}