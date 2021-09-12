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
using CalDavSynchronizer.Utilities;

namespace CalDavSynchronizer.DataAccess
{
    public class FileDataAccess<T>
    {
        private readonly string _filePath;
        private readonly Func<T> _defaultValueFactory;

        public FileDataAccess(string filePath, Func<T> defaultValueFactory)
        {
            _filePath = filePath;
            _defaultValueFactory = defaultValueFactory;
        }

        public T Load()
        {
            if (!File.Exists(_filePath))
                return _defaultValueFactory();
            else
                return Serializer<T>.Deserialize(File.ReadAllText(_filePath));
        }

        public void Save(T value)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));

            File.WriteAllText(_filePath, Serializer<T>.Serialize(value));
        }

        public void EnsureBackupExists(string backupName)
        {
            if (File.Exists(_filePath))
            {
                var backupFileName = $"{_filePath}.{backupName}.bak";
                if (!File.Exists(backupFileName))
                {
                    File.Copy(_filePath, backupFileName, true);
                }
            }
        }
    }
}