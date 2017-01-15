
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thought.vCards;
using Assert = NUnit.Framework.Assert;

namespace Tests
{
    [TestClass]
    public sealed class vCardStandardWriterTests : IDisposable
    {

        // The next set of methods test the EncodeEscaped
        // function.  This function encodes certain characters
        // into text sequences (e.g. a semicolon into "\;") to
        // avoid parsing problems.

        #region [ EncodeEscaped_Comma ]

        [TestMethod]
        public void EncodeEscaped_Comma()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                @"\,",
                writer.EncodeEscaped(","));

        }

        #endregion

        #region [ EncodeEscaped_Comma_Comma ]

        [TestMethod]
        public void EncodeEscaped_Comma_Comma()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                @"\,\,",
                writer.EncodeEscaped(",,"));

        }

        #endregion

        #region [ EncodeEscaped_Comma_Text ]

        [TestMethod]
        public void EncodeEscaped_Comma_Text()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                @"\,text",
                writer.EncodeEscaped(",text"));

        }

        #endregion

        #region [ EncodeEscaped_Comma_Text_Comma ]

        [TestMethod]
        public void EncodeEscaped_Comma_Text_Comma()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                @"\,text\,",
                writer.EncodeEscaped(",text,"));

        }

        #endregion

        #region [ EncodeEscaped_CRLF ]

        [TestMethod]
        public void EncodeEscaped_CRLF()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                @"\r\n",
                writer.EncodeEscaped("\r\n"));

        }

        #endregion

        #region [ EncodeEscaped_CRLF_Text ]

        [TestMethod]
        public void EncodeEscaped_CRLF_Text()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                @"\r\ntext",
                writer.EncodeEscaped("\r\ntext"));

        }

        #endregion

        #region [ EncodeEscaped_Empty ]

        [TestMethod]
        public void EncodeEscaped_Empty()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                string.Empty,
                writer.EncodeEscaped(string.Empty));

        }

        #endregion

        #region [ EncodeEscaped_Null ]

        [TestMethod]
        public void EncodeEscaped_Null()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                null,
                writer.EncodeEscaped((string)null));

        }

        #endregion

        #region [ EncodeEscaped_Semicolon ]

        [TestMethod]
        public void EncodeEscaped_Semicolon()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                @"\;",
                writer.EncodeEscaped(";"));

        }

        #endregion

        #region [ EncodeEscaped_Semicolon_Space_Semicolon ]

        [TestMethod]
        public void EncodeEscaped_Semicolon_Space_Semicolon()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                @"\; \;",
                writer.EncodeEscaped("; ;"));

        }

        #endregion

        #region [ EncodeEscaped_Semicolon_CRLF ]

        [TestMethod]
        public void EncodeEscaped_Semicolon_CRLF()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                @"\;\r\n",
                writer.EncodeEscaped(";\r\n"));

        }

        #endregion

        #region [ EncodeEscaped_Text_Comma ]

        [TestMethod]
        public void EncodeEscaped_Text_Comma()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                @"text\,",
                writer.EncodeEscaped("text,"));

        }

        #endregion

        #region [ EncodeEscaped_Text_Comma_Text ]

        [TestMethod]
        public void EncodeEscaped_Text_Comma_Text()
        {

            vCardStandardWriter writer = new vCardStandardWriter();

            Assert.AreEqual(
                @"text\,text",
                writer.EncodeEscaped("text,text"));

        }

        #endregion

        // The next set of tests check the EncodeProperty function
        // against a property that has been initialized in various
        // ways.  For example, the EncodeProperty_Name_Subproperty
        // test checks the encoding of a property that has a name
        // and subproperty, but no value or subproperty value.
        // All common variations are tested.

        #region [ EncodeProperty_Name ]

        [TestMethod]
        public void EncodeProperty_Name()
        {

            vCardStandardWriter writer =
                new vCardStandardWriter();

            vCardProperty property =
                new vCardProperty("NAME");

            Assert.AreEqual(
                "NAME:",
                writer.EncodeProperty(property));

        }

        #endregion

        #region [ EncodeProperty_Name_Subproperties ]

        [TestMethod]
        public void EncodeProperty_Name_Subproperties()
        {

            vCardStandardWriter writer =
                new vCardStandardWriter();

            vCardProperty property =
                new vCardProperty("NAME");

            property.Subproperties.Add("SUB1");
            property.Subproperties.Add("SUB2");

            Assert.AreEqual(
                "NAME;SUB1;SUB2:",
                writer.EncodeProperty(property));

        }

        #endregion

        #region [ EncodeProperty_Name_Subproperties_Value ]

        [TestMethod]
        public void EncodeProperty_Name_Subproperties_Value()
        {

            vCardStandardWriter writer =
                new vCardStandardWriter();

            vCardProperty property =
                new vCardProperty("NAME","VALUE");

            property.Subproperties.Add("SUB1");
            property.Subproperties.Add("SUB2");

            Assert.AreEqual(
                "NAME;SUB1;SUB2:VALUE",
                writer.EncodeProperty(property));

        }

        #endregion

        #region [ EncodeProperty_Name_Subproperty ]

        [TestMethod]
        public void EncodeProperty_Name_Subproperty()
        {

            vCardStandardWriter writer =
                new vCardStandardWriter();

            vCardProperty property =
                new vCardProperty("NAME");

            property.Subproperties.Add("SUB");

            Assert.AreEqual(
                "NAME;SUB:",
                writer.EncodeProperty(property));

        }

        #endregion

        #region [ EncodeProperty_Name_Subproperty_Value ]

        [TestMethod]
        public void EncodeProperty_Name_Subproperty_Value()
        {

            vCardStandardWriter writer =
                new vCardStandardWriter();

            vCardProperty property =
                new vCardProperty("NAME", "VALUE");

            property.Subproperties.Add("SUB");

            Assert.AreEqual(
                "NAME;SUB:VALUE",
                writer.EncodeProperty(property));

        }

        #endregion

        #region [ EncodeProperty_Name_Subproperty_Subvalue ]

        [TestMethod]
        public void EncodeProperty_Name_Subproperty_Subvalue()
        {

            vCardStandardWriter writer =
                new vCardStandardWriter();

            vCardProperty property =
                new vCardProperty("NAME");

            property.Subproperties.Add("SUB", "SUBVALUE");

            Assert.AreEqual(
                "NAME;SUB=SUBVALUE:",
                writer.EncodeProperty(property));

        }

        #endregion

        #region [ EncodeProperty_Name_Subproperty_Subvalue_Value ]

        [TestMethod]
        public void EncodeProperty_Name_Subproperty_Subvalue_Value()
        {

            vCardStandardWriter writer =
                new vCardStandardWriter();

            vCardProperty property =
                new vCardProperty("NAME", "VALUE");

            property.Subproperties.Add("SUB", "SUBVALUE");

            Assert.AreEqual(
                "NAME;SUB=SUBVALUE:VALUE",
                writer.EncodeProperty(property));

        }

        #endregion

        #region [ EncodeProperty_Name_Subproperty_Subvalue_Subproperty ]

        [TestMethod]
        public void EncodeProperty_Name_Subproperty_Subvalue_Subproperty()
        {

            vCardStandardWriter writer =
                new vCardStandardWriter();

            vCardProperty property =
                new vCardProperty("NAME");

            property.Subproperties.Add("SUB1", "SUBVALUE");
            property.Subproperties.Add("SUB2");

            Assert.AreEqual(
                "NAME;SUB1=SUBVALUE;SUB2:",
                writer.EncodeProperty(property));

        }

        #endregion

        #region [ EncodeProperty_Name_Subproperty_Subvalue_Subproperty_Value ]

        [TestMethod]
        public void EncodeProperty_Name_Subproperty_Subvalue_Subproperty_Value()
        {

            vCardStandardWriter writer =
                new vCardStandardWriter();

            vCardProperty property =
                new vCardProperty("NAME", "VALUE");

            property.Subproperties.Add("SUB1", "SUBVALUE");
            property.Subproperties.Add("SUB2");

            Assert.AreEqual(
                "NAME;SUB1=SUBVALUE;SUB2:VALUE",
                writer.EncodeProperty(property));

        }

        #endregion

        #region [ EncodeProperty_Name_Value ]

        [TestMethod]
        public void EncodeProperty_Name_Value()
        {

            vCardStandardWriter writer =
                new vCardStandardWriter();

            vCardProperty property =
                new vCardProperty("NAME", "VALUE");

            Assert.AreEqual(
                "NAME:VALUE",
                writer.EncodeProperty(property));

        }

        #endregion

        #region [ EncodeProperty_Null ]

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void EncodeProperty_Null()
        {

            vCardStandardWriter writer =
                new vCardStandardWriter();

            writer.EncodeProperty(null);

        }

        #endregion

        #region [ EncodeQuotedPrintable_CRLF ]

        [TestMethod]
        public void EncodeQuotedPrintable_CRLF()
        {

            Assert.AreEqual(
                "=0D=0A",
                vCardStandardWriter.EncodeQuotedPrintable("\r\n"));

        }

        #endregion

        #region [ EncodeQuotedPrintable_Empty ]

        [TestMethod]
        public void EncodeQuotedPrintable_Empty()
        {

            Assert.IsEmpty(
                vCardStandardWriter.EncodeQuotedPrintable(string.Empty));

        }

        #endregion

        #region [ EncodeQuotedPrintable_Null ]

        [TestMethod]
        public void EncodeQuotedPrintable_Null()
        {

            // The function should return null if null is
            // passed to it.  A future version might return
            // String.Empty or raise an exception; the best
            // behavior is under investigation.

            Assert.IsNull(
                vCardStandardWriter.EncodeQuotedPrintable((string)null));
        }

        #endregion
        public void Dispose()
        { // driver.Dispose(); 
        }
    }

 

}
