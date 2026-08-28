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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.ProfileTypes;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
    public class OXInfoDialogViewModel : ModelBase
    {
        public event EventHandler<CloseEventArgs> CloseRequested;
        private readonly IUiService _uiService;

        private string _infoText;

        public OXInfoDialogViewModel(IUiService uiService)
        {
            if (uiService == null) throw new ArgumentNullException(nameof(uiService));

            _uiService = uiService;

            OkCommand = new DelegateCommand(_ => Close(true));

            InfoText = Strings.Get($@"You want to use Outlook with your OX App Suite account?

Fuago GmbH now offers Outlook Sync for OX.
With Outlook Sync for OX you get access to exclusive Features.

Exclusive with Outlook Sync for OX: 
-predefined profile configurations
- synchronization of OX App Suite distribution lists
- support

Get in contact with OSfO@fuago.io, today.

Of course you can still set up a free generic profile and manage all settings manually.");
        }

        public ICommand OkCommand { get; }

        public string InfoText
        {
            get => _infoText;
            set => CheckedPropertyChange(ref _infoText, value);
        }

        void Close(bool okPressed)
        {
            CloseRequested?.Invoke(this, new CloseEventArgs(okPressed));
        }

        public static OXInfoDialogViewModel DesignInstance => new OXInfoDialogViewModel(NullUiService.Instance);
    }
}