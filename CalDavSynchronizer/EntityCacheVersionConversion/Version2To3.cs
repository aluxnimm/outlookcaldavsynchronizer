using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation.ComWrappers;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.EntityCacheVersionConversion
{
    public static class Version2To3
    {
        private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        public static void Convert(
            NameSpace outlookSession,
            Options[] options,
            Func<Options, string> cacheFileGetter,
            Action<Options> cacheDeleter)
        {
            foreach (var option in options)
            {
                try
                {
                    Convert(outlookSession, option, cacheFileGetter, cacheDeleter);
                }
                catch (System.Exception x)
                {
                    s_logger.Error($"Error during conversion for profile '{option.Name}'. Deleting caches", x);
                    cacheDeleter(option);
                }
            }
        }

        private static void Convert(NameSpace outlookSession, Options options, Func<Options, string> cacheFileGetter, Action<Options> cacheDeleter)
        {
            OlItemType defaultItemType;

            using (var outlookFolderWrapper = GenericComObjectWrapper.Create((Folder) outlookSession.GetFolderFromID(options.OutlookFolderEntryId, options.OutlookFolderStoreId)))
            {
                defaultItemType = outlookFolderWrapper.Inner.DefaultItemType;
            }

            if (defaultItemType == OlItemType.olTaskItem)
            {
                var fileName = cacheFileGetter(options);
                XDocument document = XDocument.Load(fileName);

                if (document.Root?.Name.LocalName == "ArrayOfOutlookEventRelationData")
                {
                    document.Root.Name = "ArrayOfTaskRelationData";
                    foreach (var node in document.Descendants().Where(n => n.Name == "OutlookEventRelationData"))
                    {
                        node.Name = "TaskRelationData";
                    }

                    document.Save(fileName);
                }
            }
        }
    }
}