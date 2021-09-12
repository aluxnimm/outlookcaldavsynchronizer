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
using System.Reflection;
using System.Xml.Linq;
using log4net;

namespace CalDavSynchronizer.EntityCacheVersionConversion
{
    public static class Version0To1
    {
        private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        public static void Convert(IEnumerable<string> entityRelationCacheFiles)
        {
            foreach (var file in entityRelationCacheFiles)
            {
                if (File.Exists(file))
                {
                    XDocument input = XDocument.Load(file);
                    Convert(input);
                    input.Save(file);
                }
            }
        }

        //private static IEnumerable<Tuple<string, string>> CreateBackup (IEnumerable<string> entityRelationCacheFiles)
        //{
        //  var backup = new List<Tuple<string, string>>();
        //  foreach (var file in entityRelationCacheFiles)
        //  {
        //    if (File.Exists (file))
        //    {
        //      var backupFileName = Path.Combine (Path.GetDirectoryName (file), Guid.NewGuid().ToString());
        //      File.Copy (file, backupFileName);
        //      backup.Add (Tuple.Create (file, backupFileName));
        //    }
        //  }
        //  return backup;
        //}

        //private static void Undo (IEnumerable<Tuple<string, string>> backup)
        //{
        //  foreach (var backupEntry in backup)
        //  {
        //    File.Delete (backupEntry.Item1);
        //    File.Move (backupEntry.Item2, backupEntry.Item1);
        //  }
        //}

        private static void Convert(XDocument document)
        {
            var btypeNodes = document.Descendants().Where(n => n.Name == "BtypeId");

            foreach (var btypeNode in btypeNodes)
            {
                var uri = btypeNode.Value;
                btypeNode.RemoveAll();
                btypeNode.Add(new XElement("OriginalAbsolutePath", uri));
                btypeNode.Add(new XElement("Id", DecodedString(uri)));
            }
        }

        private static string DecodedString(string value)
        {
            string newValue;
            while ((newValue = Uri.UnescapeDataString(value)) != value)
                value = newValue;

            return newValue;
        }
    }
}