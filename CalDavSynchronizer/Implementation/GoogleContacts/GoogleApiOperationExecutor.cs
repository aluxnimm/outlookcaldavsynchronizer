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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Contacts;
using Google.GData.Client;
using log4net;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  class GoogleApiOperationExecutor : IGoogleApiOperationExecutor
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    readonly ContactsRequest _contactFacade;
    const int c_exponentialBackoffBaseMilliseconds = 100;
    const int c_exponentialBackoffMaxRetries = 6;
    private readonly Random _exponentialBackoffRandom = new Random();

    public GoogleApiOperationExecutor (ContactsRequest contactFacade)
    {
      if (contactFacade == null)
        throw new ArgumentNullException (nameof (contactFacade));
      _contactFacade = contactFacade;
    }

    public T Execute<T> (Func<ContactsRequest, T> operation)
    {
      for (int retryCount = 0;; retryCount++)
      {
        try
        {
          return operation (_contactFacade);
        }
        catch (GDataRequestException x) when
            ((((x.InnerException as WebException)
                ?.Response as HttpWebResponse)
                ?.StatusCode == HttpStatusCode.ServiceUnavailable) &&
             retryCount < c_exponentialBackoffMaxRetries)
        {
          var sleepMilliseconds = (int) Math.Pow (2, retryCount) * c_exponentialBackoffBaseMilliseconds + _exponentialBackoffRandom.Next (c_exponentialBackoffBaseMilliseconds);
          s_logger.Warn ($"Retrying operation in ${sleepMilliseconds}ms.");
          Thread.Sleep (sleepMilliseconds);
        }
      }
    }

    public void Execute (Action<ContactsRequest> operation)
    {
      Execute (f =>
      {
        operation (f);
        return 0;
      });
    }
  }
}