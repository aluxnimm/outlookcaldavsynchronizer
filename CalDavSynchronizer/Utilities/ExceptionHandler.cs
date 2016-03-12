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
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using log4net;
using log4net.Core;

namespace CalDavSynchronizer.Utilities
{
  public class ExceptionHandler : IExceptionHandler
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    public static readonly IExceptionHandler Instance = new ExceptionHandler();

    private ExceptionHandler ()
    {
    }

    public void DisplayException (Exception exception, ILog logger)
    {
      var additionMessage = GetAdditionMessageNoThrow (exception);
      logger.Logger.Log (typeof (ExceptionHandler), Level.Error, additionMessage, exception);
      MessageBox.Show (exception.ToString(), ComponentContainer.MessageBoxTitle);
    }

    public void LogException (Exception exception, ILog logger)
    {
      var additionMessage = GetAdditionMessageNoThrow (exception);
      logger.Logger.Log (typeof (ExceptionHandler), Level.Error, additionMessage, exception);
    }

    private string GetAdditionMessageNoThrow (Exception exception)
    {
      try
      {
        return GetAdditionalExceptionMessage (exception);
      }
      catch (Exception x)
      {
        s_logger.Error ("Exception while creating additional exception message", x);
        return string.Empty;
      }
    }

    private string GetAdditionalExceptionMessage (Exception x)
    {
      var webException = x as WebException;
      if (webException != null)
      {
        return GetAdditionalExceptionMessage (webException);
      }

      return string.Empty;
    }

    private string GetAdditionalExceptionMessage (WebException x)
    {
      StringBuilder stringBuilder = new StringBuilder();

      stringBuilder.AppendFormat ("Status: {0}", x.Status);
      stringBuilder.AppendLine();

      AppendHttpResponseDetails (stringBuilder, x);

      return stringBuilder.ToString();
    }

    private void AppendHttpResponseDetails (StringBuilder stringBuilder, WebException exception)
    {
      var httpWebResponse = exception.Response as HttpWebResponse;
      if (httpWebResponse == null)
        return;

      try
      {
        stringBuilder.AppendFormat ("StatusCode: {0}", httpWebResponse.StatusCode);
        stringBuilder.AppendLine();

        stringBuilder.AppendFormat ("StatusDescription: {0}", httpWebResponse.StatusDescription);
        stringBuilder.AppendLine();

        try
        {
          using (var reader = new StreamReader (httpWebResponse.GetResponseStream()))
          {
            stringBuilder.AppendFormat ("Body: {0}", reader.ReadToEnd());
            stringBuilder.AppendLine();
          }
        }
        catch (ProtocolViolationException)
        {
          // Occurs if there is no response stream and can be ignored
        }
      }
      catch (Exception x)
      {
        s_logger.Error ("Exception while getting exception details.", x);
      }
    }
  }
}