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
using log4net;

namespace CalDavSynchronizer.Generic.ProgressReport
{
  /// <summary>
  /// Standardimplementation of IProgressStep
  /// </summary>
  public class ProgressStep : IProgressStep
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly double _uiTicksPerStepTick;
    private readonly IProgressUi _progressUi;
    private readonly int _uiMax;
    private double _currentValue;


    public ProgressStep (IProgressUi progressUi, int uiMin, int uiMax, int stepMaximum)
    {
      _progressUi = progressUi;
      _uiTicksPerStepTick = (uiMax - uiMin) / (double) stepMaximum;
      _currentValue = uiMin;
      _uiMax = uiMax;
    }

    public void Dispose ()
    {
      try
      {
        _progressUi.SetValue (_uiMax);
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
    }

    public void Increase ()
    {
      IncreaseBy (1);
    }

    public void IncreaseBy (int value)
    {
      try
      {
        _currentValue = Math.Min (_currentValue + (value * _uiTicksPerStepTick), _uiMax);
        _progressUi.SetValue ((int)_currentValue);
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
    }
  }
}