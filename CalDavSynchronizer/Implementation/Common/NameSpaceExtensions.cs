using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Common
{
  public static class NameSpaceExtensions
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod ().DeclaringType);

    public static TItemType GetEntryOrNull<TItemType> (this NameSpace mapiNameSpace, string entryId, string storeId)
    where TItemType : class
    {
      try
      {
        var item = (TItemType) mapiNameSpace.GetItemFromID (entryId, storeId);
        return item;
      }
      catch (COMException x)
      {
        const int messageNotFoundResult = -2147221233;
        if (x.HResult != messageNotFoundResult)
          s_logger.Error ("Error while fetching entity.", x);
        return null;
      }
    }

  }
}
