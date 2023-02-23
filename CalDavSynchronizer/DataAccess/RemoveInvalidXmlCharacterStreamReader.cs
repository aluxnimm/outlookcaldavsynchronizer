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
    // used to save last char from read buffer if it is part of a surrogate pair
    private int _lastBufferChar = -1;
    private readonly char _replacementCharacter = '\uFFFD';
    public RemoveInvalidXmlCharacterStreamReader(Stream stream, Encoding encoding) : base(stream, encoding)
    {
    }
    public override int Read(char[] buffer, int index, int count)
    {
        int charsRead = base.Read(buffer, index, count);
        int bufferEnd = index + charsRead;

        for (int i = index; i < bufferEnd; i++)
        {
            // check if current char is valid xml
            if (!XmlConvert.IsXmlChar(buffer[i]))
            {
                // special case check if there is a surrogate pair with first char of this buffer and last char of old buffer
                if (index == i && _lastBufferChar != -1)
                {
                    if (!XmlConvert.IsXmlSurrogatePair(buffer[i],(char)_lastBufferChar))
                    {
                        buffer[i] = _replacementCharacter;
                    }
                    _lastBufferChar = -1;
                }
                // special case check if there is a surrogate pair with last char of this buffer and first char of next buffer
                else if (bufferEnd == i + 1)
                {
                    int nextBufferFirstChar = Peek();
                    if (XmlConvert.IsXmlSurrogatePair(nextBufferFirstChar != -1 ? (char)nextBufferFirstChar : '\0', buffer[i]))
                    {
                        _lastBufferChar = buffer[i];
                    }
                    else
                    {
                        buffer[i] = _replacementCharacter;
                    }
                }
                // skip next character validation if it is a surrogate pair
                else if (XmlConvert.IsXmlSurrogatePair(buffer[i + 1], buffer[i]))
                {
                    ++i;
                }
                else
                {
                    // If the current character is not a valid XML character and not inside a surrogate pair, replace it with a the unicode replacement character.
                    buffer[i] = _replacementCharacter;
                }
            }
        }
        return charsRead;
    }
}