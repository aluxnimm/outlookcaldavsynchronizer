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

    private const int c_aloadStepFactor = 1;
    private const int c_bloadStep1Factor = 2;
    private const int c_bloadStep2Factor = 16;
    private const int c_syncStepFactor = 39;

    private readonly int[] _prefixSummedStepTotals;
    private readonly IProgressUi _progressUi;
    private int _currentStep = -1;

    public TotalProgressLogger (IProgressUiFactory uiFactory, int aAnnounced, int bAnnounced)
    {
      const int smallStepCompletionCountAnticipationFactor = 10;

      _prefixSummedStepTotals = new[]
                                {
                                    smallStepCompletionCountAnticipationFactor * c_aloadStepFactor * aAnnounced,
                                    smallStepCompletionCountAnticipationFactor * c_bloadStep1Factor * bAnnounced,
                                    smallStepCompletionCountAnticipationFactor * c_bloadStep2Factor * bAnnounced,
                                    smallStepCompletionCountAnticipationFactor * c_syncStepFactor * (aAnnounced + bAnnounced),
                                };

      CalculatePrefixSum (_prefixSummedStepTotals);

      _progressUi = uiFactory.Create (_prefixSummedStepTotals[_prefixSummedStepTotals.Length - 1]);
    }

    private void CalculatePrefixSum (int[] values)
    {
      for (int i = 0, sum = 0; i < values.Length; i++)
      {
        sum = values[i] = sum + values[i];
      }
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

    IProgressLogger ITotalProgressLogger.StartStep (int stepCompletedCount, string stepDescription)
    {
      try
      {
        _progressUi.SetMessage (stepDescription);

        _currentStep++;
        if (_currentStep > _prefixSummedStepTotals.Length)
        {
          s_logger.ErrorFormat ("Exceeded total progress steps of {0}", _prefixSummedStepTotals.Length);
          _progressUi.SetValue (_prefixSummedStepTotals[_prefixSummedStepTotals.Length - 1]);
          return NullProgressLogger.Instance;
        }
        else
        {
          return new ProgressLogger (
              _progressUi,
              _currentStep == 0 ? 0 : _prefixSummedStepTotals[_currentStep - 1],
              _prefixSummedStepTotals[_currentStep],
              stepCompletedCount
              );
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
        return NullProgressLogger.Instance;
      }
    }

    public void NotifyLoadCount (int aLoadCount, int bLoadCount)
    {
    }
  }
}