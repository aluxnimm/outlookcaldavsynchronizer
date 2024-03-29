﻿//#define RECORD_RESOURCES

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Globalization
{
    public class Strings
    {
        public static string Get(FormattableString key)
        {
            return string.Format(Localize(key.Format), key.GetArguments());
        }

        public static string Localize(string text)
        {
#if RECORD_RESOURCES
      AddResource(text);
      return Regex.Replace(text, "[A-Za-z]", "_");
#else
            var translation = StringResources.ResourceManager.GetString(text);
            return translation ?? text;
#endif
        }


#if RECORD_RESOURCES
    private static void AddResource(string text)
    {
      const string resxFileName = @"D:\dev\CalDavSynchronizer\CalDavSynchronizer\Globalization\StringResources.de-DE.resx";
      var allResources = new ResXResourceSet(resxFileName);
      if (allResources.GetString(text) == null)
      {
        using (var writer = new ResXResourceWriter(resxFileName))
        {
          foreach (DictionaryEntry existingEntry in allResources)
          {
            writer.AddResource((string)existingEntry.Key, existingEntry.Value);
          }
          writer.AddResource(text, text);
        }
      }
    }
#endif
    }
}