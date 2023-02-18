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

using System.IO;
using System.Text;
using System.Xml;

public class RemoveInvalidXmlCharacterStreamReader : StreamReader
{
    public RemoveInvalidXmlCharacterStreamReader(Stream stream, Encoding encoding) : base(stream, encoding)
    {
    }

    public override int Peek()
    {
        var ch = base.Peek();
        if (ch != -1 && IsInvalidChar(ch))
        {
            return Peek();
        }
        return ch;
    }

    public override int Read()
    {
        var ch = base.Read();
        if (ch != -1 && IsInvalidChar(ch))
        {
            return Read();
        }
        return ch;
    }

    public override int Read(char[] buffer, int index, int count)
    {
        int readCount = 0, ch;

        for (int i = 0; i < count && (ch = Read()) != -1; i++)
        {
            readCount++;
            buffer[index + i] = (char)ch;
        }

        return readCount;
    }

    private static bool IsInvalidChar(int ch)
    {
        return IsInvalidChar((char)ch);
    }

    private static bool IsInvalidChar(char ch)
    {
        return !XmlConvert.IsXmlChar(ch);
    }
}