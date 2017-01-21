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

using System.Collections.Generic;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Ui.Options.ProfileTypes;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Ui.Reports.ViewModels;
using CalDavSynchronizer.Ui.ViewModels;
using GenSync.ProgressReport;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui
{
  class NullUiService : IUiService
  {
    public  static readonly IUiService Instance = new NullUiService();

    private NullUiService()
    {
    }

    public void Show(ReportsViewModel reportsViewModel)
    {
      
    }

    public void ShowProfileStatusesWindow()
    {
     
    }

    public bool ShowOptions(OptionsCollectionViewModel viewModel)
    {
      return false;
    }
    
    public void ShowErrorDialog(string errorMessage, string title)
    {
      
    }

    public string ShowSaveDialog(string title)
    {
      return null;
    }

    public string ShowOpenDialog(string title)
    {
      return null;
    }

    public void ShowReport (string title, string reportText)
    {
     
    }

    public IProgressUi Create(int maxValue)
    {
      return null;
    }

    public IProfileType QueryProfileType(IReadOnlyCollection<IProfileType> profileTypes)
    {
      return null;
    }
  }
}