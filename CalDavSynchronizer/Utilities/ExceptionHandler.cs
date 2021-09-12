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
        private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        public static readonly IExceptionHandler Instance = new ExceptionHandler();

        private ExceptionHandler()
        {
        }

        public void DisplayException(Exception exception, ILog logger)
        {
            logger.Logger.Log(typeof(ExceptionHandler), Level.Error, string.Empty, exception);
            MessageBox.Show(exception.ToString(), ComponentContainer.MessageBoxTitle);
        }

        public void LogException(Exception exception, ILog logger)
        {
            LogException(string.Empty, exception, logger);
        }

        public void LogException(string message, Exception exception, ILog logger)
        {
            logger.Logger.Log(typeof(ExceptionHandler), Level.Error, message, exception);
        }
    }
}