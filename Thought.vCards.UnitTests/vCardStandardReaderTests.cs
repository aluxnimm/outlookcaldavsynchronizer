using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thought.vCards;
using Assert = NUnit.Framework.Assert;

namespace Tests
{
    [TestClass]
    public sealed class vCardStandardReaderTests : IDisposable
    {
        private const string TestName = "NAME";
        private const string TestValue = "VALUE";

        #region [ DecodeEscaped_Comma ]

        [TestMethod]
        public void DecodeEscaped_Comma()
        {
            Assert.AreEqual(
                ",",
                vCardStandardReader.DecodeEscaped(","),
                "A sole comma should be ignored as bad formatting.");
        }

        #endregion

        #region [ DecodeEscaped_Comma_SlashComma ]

        [TestMethod]
        public void DecodeEscaped_Comma_SlashComma()
        {
            // The \, sequence should be collapsed to a single comma.

            Assert.AreEqual(
                ",,",
                vCardStandardReader.DecodeEscaped(@",\,"));
        }

        #endregion

        #region [ DecodeEscaped_Comma_SlashComma_Comma ]

        [TestMethod]
        public void DecodeEscaped_Comma_SlashComma_Comma()
        {
            // The \, sequence should be collapsed to a single comma.

            Assert.AreEqual(
                ",,,",
                vCardStandardReader.DecodeEscaped(@",\,,"));
        }

        #endregion

        #region [ DecodeEscaped_Empty ]

        [TestMethod]
        public void DecodeEscaped_Empty()
        {
            Assert.IsEmpty(
                vCardStandardReader.DecodeEscaped(string.Empty));
        }

        #endregion

        #region [ DecodeEscaped_SlashComma ]

        [TestMethod]
        public void DecodeEscaped_SlashComma()
        {
            Assert.AreEqual(
                ",",
                vCardStandardReader.DecodeEscaped("\\,"));
        }

        #endregion

        #region [ DecodeEscaped_Null ]

        [TestMethod]
        public void DecodeEscaped_Null()
        {
            Assert.IsNull(
                vCardStandardReader.DecodeEscaped((string) null));
        }

        #endregion

        [TestMethod]
        public void DecodeEscaped_Sample()
        {
            // The encoded string below caused an out of memory
            // exception in the code as of 8/26/2007.  The problem
            // was due a failure to increment the index (in DecodeEscaped)
            // when an invalid escape character was encountered.  The \r
            // was not recognized (and treated as invalid).

            const string address =
                @"129 15th Street #3\r\nMinneapolis\, MN 55403\r\nUnited States of America";

            vCardStandardReader.DecodeEscaped(address);
        }

        #region [ DecodeHexadecimal ]

        [TestMethod]
        public void DecodeHexadecimal()
        {
            // This test ensures the DecodeHexadecimal
            // function is properly converting every
            // possible hexadecimal character.

            Assert.AreEqual(0,
                vCardStandardReader.DecodeHexadecimal('0'));

            Assert.AreEqual(1,
                vCardStandardReader.DecodeHexadecimal('1'));

            Assert.AreEqual(2,
                vCardStandardReader.DecodeHexadecimal('2'));

            Assert.AreEqual(3,
                vCardStandardReader.DecodeHexadecimal('3'));

            Assert.AreEqual(4,
                vCardStandardReader.DecodeHexadecimal('4'));

            Assert.AreEqual(5,
                vCardStandardReader.DecodeHexadecimal('5'));

            Assert.AreEqual(6,
                vCardStandardReader.DecodeHexadecimal('6'));

            Assert.AreEqual(7,
                vCardStandardReader.DecodeHexadecimal('7'));

            Assert.AreEqual(8,
                vCardStandardReader.DecodeHexadecimal('8'));

            Assert.AreEqual(9,
                vCardStandardReader.DecodeHexadecimal('9'));

            Assert.AreEqual(10,
                vCardStandardReader.DecodeHexadecimal('A'));

            Assert.AreEqual(10,
                vCardStandardReader.DecodeHexadecimal('a'));

            Assert.AreEqual(11,
                vCardStandardReader.DecodeHexadecimal('B'));

            Assert.AreEqual(11,
                vCardStandardReader.DecodeHexadecimal('b'));

            Assert.AreEqual(12,
                vCardStandardReader.DecodeHexadecimal('C'));

            Assert.AreEqual(12,
                vCardStandardReader.DecodeHexadecimal('c'));

            Assert.AreEqual(13,
                vCardStandardReader.DecodeHexadecimal('D'));

            Assert.AreEqual(13,
                vCardStandardReader.DecodeHexadecimal('d'));

            Assert.AreEqual(14,
                vCardStandardReader.DecodeHexadecimal('E'));

            Assert.AreEqual(14,
                vCardStandardReader.DecodeHexadecimal('e'));

            Assert.AreEqual(15,
                vCardStandardReader.DecodeHexadecimal('F'));

            Assert.AreEqual(15,
                vCardStandardReader.DecodeHexadecimal('f'));
        }

        #endregion

        #region [ DecodeHexadecimal_BadCharacter ]

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void DecodeHexadecimal_BadCharacter()
        {
            vCardStandardReader.DecodeHexadecimal('V');
        }

        #endregion

        #region [ DecodeQuotedPrintable_Empty ]

        [TestMethod]
        public void DecodeQuotedPrintable_Empty()
        {
            // Empty should be returned if Empty is specified.

            Assert.IsEmpty(
                vCardStandardReader.DecodeQuotedPrintable(string.Empty));
        }

        #endregion

        #region [ DecodeQuotedPrintable_EqualSign_EqualSign ]

        [TestMethod]
        public void DecodeQuotedPrintable_EqualSign_EqualSign()
        {
            Assert.AreEqual(
                "==",
                vCardStandardReader.DecodeQuotedPrintable("=="),
                "The two (invalid) equal signs are treated as raw data.");
        }

        #endregion

        #region [ DecodeQuotedPrintable_EqualSign_MoreLines ]

        [TestMethod]
        public void DecodeQuotedPrintable_EqualSign_MoreLines()
        {
            const string encodedLine =
                "Line1 Is Data1=0D=0A=\r\nLine2 Is Data2";

            const string decodedLine =
                "Line1 Is Data1\r\nLine2 Is Data2";

            Assert.AreEqual(
                decodedLine,
                vCardStandardReader.DecodeQuotedPrintable(encodedLine));
        }

        #endregion

        #region [ DecodeQuotedPrintable_EqualSign_NoMoreLines ]

        [TestMethod]
        public void DecodeQuotedPrintable_EqualSign_NoMoreLines()
        {
            // A single equal sign can be written at the end
            // of a line.  This is a hint to the decoder that
            // the data continues on the next line.  However,
            // an equal sign at the very end of a line, with
            // no more lines, it invalid.  The decoder should treat
            // it as a bad sequence and output it as raw text.

            Assert.AreEqual(
                "=",
                vCardStandardReader.DecodeQuotedPrintable("="),
                "The invalid equal sign is treated as raw data.");
        }

        #endregion

        #region [ DecodeQuotedPrintable_EqualSign_ValidEscapeCode ]

        [TestMethod]
        public void DecodeQuotedPrintable_EqualSign_ValidEscapeCode()
        {
            // An equal sign marks the beginning of a hexadecimal
            // escape sequence.  If an equal sign is followed by
            // another equal sign, then the first one is treated
            // as raw data.

            Assert.AreEqual(
                "=\r",
                vCardStandardReader.DecodeQuotedPrintable("==0D"),
                "The invalid equal sign is treated as raw data.");
        }

        #endregion

        #region [ DecodeQuotedPrintable_IncompleteEscapeCode ]

        [TestMethod]
        public void DecodeQuotedPrintable_IncompleteEscapeCode()
        {
            Assert.AreEqual(
                "=A",
                vCardStandardReader.DecodeQuotedPrintable("=A"),
                "The sequence is incomplete and therefore treated as raw data.");
        }

        #endregion

        #region [ DecodeQuotedPrintable_InvalidEscapeCode ]

        [TestMethod]
        public void DecodeQuotedPrintable_InvalidEscapeCode()
        {
            Assert.AreEqual(
                "=AXA\r",
                vCardStandardReader.DecodeQuotedPrintable("=AXA=0D"),
                "The bad sequence is invalid and therefore treated as raw data.");
        }

        #endregion

        #region [ DecodeQuotedPrintable_Null ]

        [TestMethod]
        public void DecodeQuotedPrintable_Null()
        {
            // Null should be returned if Null is specified.

            Assert.IsNull(
                vCardStandardReader.DecodeQuotedPrintable(null));
        }

        #endregion

        #region [ ReadProperty_String_EmptyParameter ]

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void ReadProperty_String_EmptyParameter()
        {
            vCardStandardReader reader = new vCardStandardReader();
            reader.ReadProperty(string.Empty);
        }

        #endregion

        #region [ ReadProperty_String_MissingName ]

        [TestMethod]
        public void ReadProperty_String_MissingName()
        {
            vCardStandardReader reader = new vCardStandardReader();
            reader.ReadProperty(":VALUE");

            Assert.AreEqual(
                1,
                reader.Warnings.Count,
                "A missing name should have caused a warning.");
        }

        #endregion

        #region [ ReadProperty_String_MissingColon ]

        [TestMethod]
        public void ReadProperty_String_MissingColon()
        {
            vCardStandardReader reader = new vCardStandardReader();
            reader.ReadProperty("TEL 911");

            Assert.AreEqual(
                1,
                reader.Warnings.Count,
                "A missing colon should have caused a warning.");
        }

        #endregion

        #region [ ReadProperty_String_NullParameter ]

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void ReadProperty_String_NullParameter()
        {
            vCardStandardReader reader = new vCardStandardReader();
            reader.ReadProperty((string) null);
        }

        #endregion

        #region [ ReadProperty_String_QuotedPrintable ]

        [TestMethod]
        public void ReadProperty_String_QuotedPrintable()
        {
            const string encodedValue =
                "LABEL;" +
                "HOME;" +
                "ENCODING=QUOTED-PRINTABLE:" +
                "129 15th Street #3=0D=0A" +
                "Minneapolis, MN 55403=0D=0A" +
                "United States of America";

            const string decodedValue =
                "129 15th Street #3\r\n" +
                "Minneapolis, MN 55403\r\n" +
                "United States of America";

            vCardStandardReader reader =
                new vCardStandardReader();

            // Read the property string.  It should
            // be decoded by the reader.

            vCardProperty property =
                reader.ReadProperty(encodedValue);

            Assert.AreEqual(
                "LABEL",
                property.Name,
                "The name of the property should be LABEL.");

            Assert.IsTrue(
                property.Subproperties.Contains("HOME"),
                "The property should have a subproperty called HOME.");

            // Now for the big test.  The loaded property
            // value should be decoded.  It should not have the
            // quoted-printable escape sequences.

            Assert.AreEqual(
                decodedValue,
                property.ToString());
        }

        #endregion

        #region [ ReadProperty_String_SingleColon ]

        [TestMethod]
        public void ReadProperty_String_SingleColon()
        {
            vCardStandardReader reader = new vCardStandardReader();
            reader.ReadProperty(":");

            Assert.AreEqual(
                1,
                reader.Warnings.Count,
                "A single colon should have caused a warning.");
        }

        #endregion

        #region [ ReadProperty_String_Whitespace ]

        [TestMethod]
        public void ReadProperty_String_Whitespace()
        {
            vCardStandardReader reader = new vCardStandardReader();
            reader.ReadProperty("  ");

            Assert.AreEqual(
                1,
                reader.Warnings.Count,
                "A string with only whitespace should have caused a warning.");
        }

        #endregion

        #region [ ReadProperty_String_Name_Value ]

        [TestMethod]
        public void ReadProperty_String_Name_Value()
        {
            // This function tests the parsing function
            // against a basic string like NAME:VALUE.

            vCardStandardReader reader = new vCardStandardReader();

            vCardProperty property = reader.ReadProperty(
                TestName + ":" + TestValue);

            Assert.AreEqual(
                TestName,
                property.Name);

            Assert.AreEqual(
                TestValue,
                property.Value,
                "The parsed value is incorrect.");

            Assert.IsEmpty(
                property.Subproperties,
                "The Subproperties collection should be empty.");
        }

        #endregion

        #region [ ReadProperty_String_Name_Subproperties_Value ]

        [TestMethod]
        public void ReadProperty_String_Name_Subproperties_Value()
        {
            // This function tests the parser against a property
            // string with two subproperties.

            vCardStandardReader reader =
                new vCardStandardReader();

            vCardProperty property =
                reader.ReadProperty("NAME;SUB1;SUB2:VALUE");

            Assert.AreEqual(
                "NAME",
                property.Name,
                "The Name is incorrect.");

            Assert.AreEqual(
                2,
                property.Subproperties.Count,
                "The Subproperties collection has an incorrect number of items.");

            Assert.AreEqual(
                "SUB1",
                property.Subproperties[0].Name,
                "The Subproperty[0] value is incorrect.");

            Assert.AreEqual(
                "SUB2",
                property.Subproperties[1].Name,
                "The Subproperty[1] value is incorrect.");

            Assert.AreEqual(
                "VALUE",
                property.Value,
                "The parsed value is incorrect.");
        }

        #endregion

        #region [ ReadProperty_String_Name_Subproperty_Value ]

        [TestMethod]
        public void ReadProperty_String_Name_Subproperty_Value()
        {
            vCardStandardReader reader =
                new vCardStandardReader();

            vCardProperty property =
                reader.ReadProperty("NAME;SUB:VALUE");

            Assert.AreEqual(
                "NAME",
                property.Name,
                "The Name is incorrect.");

            Assert.AreEqual(
                1,
                property.Subproperties.Count,
                "The Subproperties collection has an incorrect number of items.");

            Assert.AreEqual(
                "SUB",
                property.Subproperties[0].Name,
                "The Subproperty value is incorrect.");

            Assert.AreEqual(
                "VALUE",
                property.Value,
                "The parsed value is incorrect.");
        }

        #endregion

        #region [ ReadProperty_String_Name_Subproperty_Subvalue_Value ]

        public void ReadProperty_String_Name_Subproperty_Subvalue_Value()
        {
            vCardStandardReader reader =
                new vCardStandardReader();

            vCardProperty property =
                reader.ReadProperty("TEL;TYPE=WORK:800-929-5805");

            Assert.AreEqual(
                "TEL",
                property.Name,
                "The name of the property should be TEL");

            Assert.AreEqual(
                1,
                property.Subproperties.Count,
                "There should be exactly one subproperty.");

            Assert.AreEqual(
                "TYPE",
                property.Subproperties[0].Name,
                "The name of the subproperty should be TYPE.");

            Assert.AreEqual(
                "WORK",
                property.Subproperties[0].Value,
                "The value of the subproperty should be WORK.");

            Assert.AreEqual(
                "800-929-5805",
                property.Value,
                "The value of the property is not correct.");
        }

        #endregion

        public void Dispose()
        {
            // driver.Dispose(); 
        }
    }
}