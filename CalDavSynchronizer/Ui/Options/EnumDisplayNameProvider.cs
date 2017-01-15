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
using CalDavSynchronizer.Implementation;

namespace CalDavSynchronizer.Ui.Options
{
  public class EnumDisplayNameProvider : IEnumDisplayNameProvider
  {
    private EnumDisplayNameProvider()
    {
    }

    public static readonly IEnumDisplayNameProvider Instance = new EnumDisplayNameProvider();

    public string Get(ConflictResolution value)
    {
      switch (value)
      {
        case ConflictResolution.Automatic:
          return "Automatic";
        case ConflictResolution.Manual:
          return "Manual";
        case ConflictResolution.ServerWins:
          return "Server wins";
        case ConflictResolution.OutlookWins:
          return "Outlook wins";
        default:
          throw new NotImplementedException($"Value '{value}' not implemented.");
      }
    }

    public string Get(SynchronizationMode value)
    {
      switch (value)
      {
        case SynchronizationMode.ReplicateOutlookIntoServer:
          return "Outlook \u2192 Server (Replicate)";
        case SynchronizationMode.ReplicateServerIntoOutlook:
          return "Outlook \u2190 Server (Replicate)";
        case SynchronizationMode.MergeOutlookIntoServer:
          return "Outlook \u2192 Server (Merge)";
        case SynchronizationMode.MergeServerIntoOutlook:
          return "Outlook \u2190 Server (Merge)";
        case SynchronizationMode.MergeInBothDirections:
          return "Outlook \u2190\u2192 Server (Two-Way)";
        default:
          throw new NotImplementedException($"Value '{value}' not implemented.");
      }
    }
  }
}