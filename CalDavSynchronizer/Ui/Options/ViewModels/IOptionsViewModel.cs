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
using System.Text;
using CalDavSynchronizer.Ui.Options.Models;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public interface IOptionsViewModel : ITreeNodeViewModel
  {
    bool IsActive { get; set; }
    bool SupportsIsActive { get; }

    IViewOptions ViewOptions { get; }

    new IEnumerable<ISubOptionsViewModel> Items { get; }

    OptionsModel Model { get; }
    bool Validate (StringBuilder errorMessageBuilder);

    OlItemType? OutlookFolderType {get;}
    bool IsMultipleOptionsTemplateViewModel { get; }
  }
}