﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
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
using System.Linq;
using System.Reflection;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Events
{
    public class ColorCategoryMapper : IColorCategoryMapper
    {
        private static readonly ILog s_logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IOutlookSession _outlookSession;
        private readonly IColorMappingsDataAccess _colorMappingsDataAccess;

        private readonly Dictionary<string, OlCategoryColor> _outlookColorByCategory;
        private readonly Dictionary<string, ColorCategoryMapping> _categoryByHtmlColor;

        public ColorCategoryMapper(IOutlookSession outlookSession, IColorMappingsDataAccess colorMappingsDataAccess)
        {
            _colorMappingsDataAccess = colorMappingsDataAccess;
            _outlookSession = outlookSession ?? throw new ArgumentNullException(nameof(outlookSession));

            var mappingEntries = colorMappingsDataAccess.Load();
            _categoryByHtmlColor = mappingEntries.ToDictionary(e => e.HtmlColor, StringComparer.OrdinalIgnoreCase);

            _outlookColorByCategory = outlookSession.GetCategories()
                .Where(c => c.Color != OlCategoryColor.olCategoryColorNone)
                .ToDictionary(c => c.Name, c => c.Color, StringComparer.InvariantCultureIgnoreCase);
        }

        public string MapHtmlColorToCategoryOrNull(string htmlColor, IEntitySynchronizationLogger logger)
        {
            var categoryColor = ColorMapper.MapHtmlColorToCategoryColor(htmlColor);

            if (_categoryByHtmlColor.TryGetValue(htmlColor, out var mapping))
            {
                var mappedCategory = mapping.PreferredCategory;

                if (_outlookColorByCategory.TryGetValue(mappedCategory, out var colorOfPreferredCategory))
                {
                    if (colorOfPreferredCategory == categoryColor)
                        return mappedCategory;
                }
            }

            string category = null;
            foreach (var entry in _outlookColorByCategory)
            {
                if (entry.Value == categoryColor)
                {
                    category = entry.Key;
                    break;
                }
            }

            if (category == null)
            {
                // No category with the required color exists. Create one with the corresponding html color name.
                category = ColorMapper.MapCategoryColorToHtmlColor(categoryColor);
                var (addCategoryResult, existingColorNameOrNull) = _outlookSession.AddCategoryNoThrow(category, categoryColor);
                switch (addCategoryResult)
                {
                    case CreateCategoryResult.DidAlreadyExist:
                        logger.LogWarning($"Did not map html color '{htmlColor}' to category '{category}', since category already exists with the wrong color ('{existingColorNameOrNull}').");
                        return null;
                    case CreateCategoryResult.Error:
                        logger.LogError($"Error while trying to create category '{category}'.");
                        return null;
                    case CreateCategoryResult.Ok:
                        _outlookColorByCategory.Add(category, categoryColor);
                        break;
                }
            }

            ColorCategoryMapping newMapping = new ColorCategoryMapping
            {
                HtmlColor = htmlColor,
                PreferredCategory = category
            };

            _categoryByHtmlColor[htmlColor] = newMapping;
            _colorMappingsDataAccess.Save(_categoryByHtmlColor.Values);

            return category;
        }

        public string MapCategoryToHtmlColorOrNull(string categoryName)
        {
            if (_outlookColorByCategory.TryGetValue(categoryName, out var color))
                return ColorMapper.MapCategoryColorToHtmlColor(color);
            else
                return null;
        }

        public static bool IsNameOfAutoGeneratedColorCategory(string category)
        {
            return ColorMapper.HtmlColorNames.Contains(category);
        }
    }
}