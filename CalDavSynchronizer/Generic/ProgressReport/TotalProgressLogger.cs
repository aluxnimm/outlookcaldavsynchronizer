// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using CalDavSynchronizer.Utilities;
using log4net;

namespace CalDavSynchronizer.Generic.ProgressReport
{
  public class TotalProgressLogger : ITotalProgressLogger
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private const int c_SingleStepTotal = 10000;


    private readonly IProgressUi _progressUi;

    public TotalProgressLogger (IProgressUiFactory uiFactory)
    {
      _progressUi = uiFactory.Create (3 * c_SingleStepTotal);
    }

    public void Dispose ()
    {
      try
      {
        _progressUi.Dispose();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
      }
    }

    public IDisposable StartARepositoryLoad ()
    {
      _progressUi.SetMessage ("Loading entities from Outlook...");
      return new ProgressLogger (_progressUi, 0, c_SingleStepTotal, 1);
    }

    public IDisposable StartBRepositoryLoad ()
    {
      _progressUi.SetMessage ("Loading entities from CalDav-Server...");
      return new ProgressLogger (_progressUi, c_SingleStepTotal, 2 * c_SingleStepTotal, 1);
    }

    public IProgressLogger StartProcessing (int entityCount)
    {
      _progressUi.SetMessage (string.Format ("Processing {0} entities...", entityCount));
      return new ProgressLogger (_progressUi, 2 * c_SingleStepTotal, 3 * c_SingleStepTotal, entityCount);
    }

    public void NotifyLoadCount (int aLoadCount, int bLoadCount)
    {
    }
  }
}