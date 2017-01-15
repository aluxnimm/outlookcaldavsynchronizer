
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thought.vCards;
using Assert = NUnit.Framework.Assert;

namespace Tests
{

    [TestClass]
    public sealed class vCardTests : IDisposable
    {

        // The next set of tests valid each simple property
        // of the vCard class.  Each test writes a value to
        // the property, then confirms the same value is
        // returned.  This is primarily intended to catch
        // capitalization types -- e.g. the code for the
        // property writes back to itself insteaed of the
        // underlying field (which causes an infinite loop).

        #region [ ReadWriteProperty_AdditionalNames ]

        [TestMethod]
        public void ReadWriteProperty_AdditionalNames()
        {

            // Make sure .AdditionalNames reads/writes

            vCard card = new vCard();
            card.AdditionalNames = "John";

            Assert.AreEqual(
                "John",
                card.AdditionalNames,
                "The AdditionalNames property is not working.");

        }

        #endregion

        #region [ ReadWriteProperty_BirthDate ]

        [TestMethod]
        public void ReadWriteProperty_BirthDate()
        {

            vCard card = new vCard();
            card.BirthDate = DateTime.Parse("04/04/04");

            Assert.AreEqual(
                DateTime.Parse("04/04/04"),
                card.BirthDate.Value,
                "The BirthDate property was not set.");

            card.BirthDate = null;

            Assert.IsNull(
                card.BirthDate,
                "The BirthDate property was not set to null.");

        }

        #endregion

        #region [ ReadWriteProperty_Department ]

        [TestMethod]
        public void ReadWriteProperty_Department()
        {

            vCard card = new vCard();
            card.Department = "DOD";

            Assert.AreEqual(
                "DOD",
                card.Department,
                "The Department property is not working.");

        }

        #endregion

        #region [ ReadWriteProperty_FamilyName ]

        [TestMethod]
        public void ReadWriteProperty_FamilyName()
        {
            
            // Make sure .FamilyName reads/writes

            vCard card = new vCard();
            card.FamilyName = "Tchaikovsky";

            Assert.AreEqual(
                "Tchaikovsky",
                card.FamilyName,
                "The FamilyName property does not return the same value written to it.");

        }

        #endregion

        #region [ ReadWriteProperty_FormattedName ]

        [TestMethod]
        public void ReadWriteProperty_FormattedName()
        {
            // Make sure .FormattedName reads/writes

            vCard card = new vCard();
            card.FormattedName = "Sir Isaac Newton";

            Assert.AreEqual(
                "Sir Isaac Newton",
                card.FormattedName,
                "The FormattedName property is not working.");

        }

        #endregion

        #region [ ReadWriteProperty_Gender ]

        [TestMethod]
        public void ReadWriteProperty_Gender()
        {

            vCard card = new vCard();
            card.Gender = vCardGender.Male;

            Assert.AreEqual(
                vCardGender.Male,
                card.Gender,
                "The Gender property is not working.");

        }

        #endregion

        #region [ ReadWriteProperty_GivenName ]

        [TestMethod]
        public void ReadWriteProperty_GivenName()
        {

            // Make sure .GivenName reads/writes

            vCard card = new vCard();
            card.GivenName = "Homer";

            Assert.AreEqual(
                "Homer",
                card.GivenName,
                "The GivenName property is not working.");
        }

        #endregion

        #region [ ReadWriteProperty_Mailer ]

        [TestMethod]
        public void ReadWriteProperty_Mailer()
        {

            vCard card = new vCard();
            card.Mailer = "RemoteAccess BBS";

            Assert.AreEqual(
                "RemoteAccess BBS",
                card.Mailer,
                "The Mailer property is not working.");
        }

        #endregion

        #region [ ReadWriteProperty_NamePrefix ]

        [TestMethod]
        public void ReadWriteProperty_NamePrefix()
        {

            vCard card = new vCard();
            card.NamePrefix = "Dr.";

            Assert.AreEqual(
                "Dr.",
                card.NamePrefix,
                "The NamePrefix property is not working.");

        }

        #endregion

        #region [ ReadWriteProperty_Office ]

        [TestMethod]
        public void ReadWriteProperty_Office()
        {

            vCard card = new vCard();
            card.Office = "Vice President's Office";

            Assert.AreEqual(
                "Vice President's Office",
                card.Office,
                "The Office property is not working.");

        }

        #endregion

        #region [ ReadWriteProperty_Organization ]

        [TestMethod]
        public void ReadWriteProperty_Organzation()
        {

            // Make sure .Organization reads/writes

            vCard card = new vCard();
            card.Organization = "BBC";

            Assert.AreEqual(
                "BBC",
                card.Organization,
                "The Organization property is not working.");

        }

        #endregion

        #region [ ReadWriteProperty_ProductId ]

        [TestMethod]
        public void ReadWriteProperty_ProductId()
        {

            vCard card = new vCard();
            card.ProductId = "vCard Library";

            Assert.AreEqual(
                "vCard Library",
                card.ProductId,
                "The ProductId property is not working.");

        }

        #endregion

        #region [ ReadWriteProperty_RevisionDate ]

        [TestMethod]
        public void ReadWriteProperty_RevisionDate()
        {

            vCard card = new vCard();
            card.RevisionDate = DateTime.Parse("01/01/2001 01:01 AM");

            Assert.AreEqual(
                DateTime.Parse("01/01/2001 01:01 AM"),
                card.RevisionDate,
                "The RevisionDate property is not working.");

        // format not working right when it comes to write out the REV property
        // REV:2013-09-18T15:39:21Z
        // REV:20130918T153921Z

            DateTime date = DateTime.Parse("11/25/2012 01:01 AM");

            string revDate =  date.ToString("s") + "Z";

            Assert.AreEqual("2012-11-25T01:01:00Z", revDate);

        }

        #endregion

        #region [ ReadWriteProperty_Role ]

        [TestMethod]
        public void ReadWriteProperty_Role()
        {

            vCard card = new vCard();
            card.Role = "Candy Bar Tester";

            Assert.AreEqual(
                "Candy Bar Tester",
                card.Role,
                "The Role property is not working.");

        }

        #endregion

        #region [ ReadWriteProperty_TimeZone ]

        [TestMethod]
        public void ReadWriteProperty_TimeZone()
        {
            vCard card = new vCard();
            card.TimeZone = "-05:00";
            Assert.AreEqual(
                "-05:00",
                card.TimeZone,
                "The TimeZone property is not working.");
        }

        #endregion

        #region [ ReadWriteProperty_Title ]

        [TestMethod]
        public void ReadWriteProperty_Title()
        {

            // The sure .Title reads/writes

            vCard card = new vCard();
            card.Title = "VP of Slapping People in the Face";

            Assert.AreEqual(
                "VP of Slapping People in the Face",
                card.Title,
                "The Title property is not working.");

        }

        #endregion

        #region [ ReadWriteProperty_UniqueId ]

        [TestMethod]
        public void ReadWriteProperty_UniqueId()
        {

            vCard card = new vCard();
            card.UniqueId = "I am unique, just like everyone else.";

            Assert.AreEqual(
                "I am unique, just like everyone else.",
                card.UniqueId,
                "The UniqueId property is not working.");

        }

        #endregion

        // The next tests ensure the class complies with Microsoft
        // best practices regarding string properties.  Specifically,
        // string properties should never return null (they should
        // always return String.Empty).

        #region [ StringEmpty_AdditionalNames ]

        [TestMethod]
        public void StringEmpty_AdditionalNames()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.AdditionalNames,
                "AdditionalNames is not string.Empty.");

            card.AdditionalNames = null;

            Assert.IsEmpty(
                card.AdditionalNames,
                "AdditionalNames is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_Department ]

        [TestMethod]
        public void StringEmpty_Department()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.Department,
                "Department is not string.Empty.");

            card.Department = null;

            Assert.IsEmpty(
                card.Department,
                "Department is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_DisplayName ]

        [TestMethod]
        public void StringEmpty_DisplayName()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.DisplayName,
                "DisplayName is not string.Empty.");

            card.DisplayName = null;

            Assert.IsEmpty(
                card.DisplayName,
                "DisplayName is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_FamilyName ]

        [TestMethod]
        public void StringEmpty_FamilyName()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.FamilyName,
                "FamilyName is not string.Empty.");

            card.FamilyName = null;

            Assert.IsEmpty(
                card.FamilyName,
                "FamilyName is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_FormattedName ]

        [TestMethod]
        public void StringEmpty_FormattedName()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.FormattedName,
                "FormattedName is not string.Empty.");

            card.FormattedName = null;

            Assert.IsEmpty(
                card.FormattedName,
                "FormattedName is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_GivenName ]

        [TestMethod]
        public void StringEmpty_GivenName()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.GivenName,
                "GivenName is not string.Empty.");

            card.GivenName = null;

            Assert.IsEmpty(
                card.GivenName,
                "GivenName is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_Mailer ]

        [TestMethod]
        public void StringEmpty_Mailer()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.Mailer,
                "Mailer is not string.Empty.");

            card.Mailer = null;

            Assert.IsEmpty(
                card.Mailer,
                "Mailer is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_NamePrefix ]

        [TestMethod]
        public void StringEmpty_NamePrefix()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.NamePrefix,
                "NamePrefix is not string.Empty.");

            card.NamePrefix = null;

            Assert.IsEmpty(
                card.NamePrefix,
                "NamePrefix is not string.Empty after being assigned null.");


        }

        #endregion

        #region [ StringEmpty_NameSuffix ]

        [TestMethod]
        public void StringEmpty_NameSuffix()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.NameSuffix,
                "NameSuffix is not string.Empty.");

            card.NameSuffix = null;

            Assert.IsEmpty(
                card.NameSuffix,
                "NameSuffix is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_Office ]

        [TestMethod]
        public void StringEmpty_Office()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.Office,
                "Office is not string.Empty.");

            card.Office = null;

            Assert.IsEmpty(
                card.Office,
                "Office is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_Organization ]

        [TestMethod]
        public void StringEmpty_Organization()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.Organization,
                "Organization is not string.Empty.");

            card.Organization = null;

            Assert.IsEmpty(
                card.Organization,
                "Organization is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_ProductId ]

        [TestMethod]
        public void StringEmpty_ProductId()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.ProductId,
                "ProductId is not string.Empty.");

            card.ProductId = null;

            Assert.IsEmpty(
                card.ProductId,
                "ProductId is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_Role ]

        [TestMethod]
        public void StringEmpty_Role()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.Role,
                "Role is not string.Empty.");

            card.Role = null;

            Assert.IsEmpty(
                card.Role,
                "Role is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_TimeZone ]

        [TestMethod]
        public void StringEmpty_TimeZone()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.TimeZone,
                "TimeZone is not string.Empty.");

            card.TimeZone = null;

            Assert.IsEmpty(
                card.TimeZone,
                "TimeZone is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_Title ]

        [TestMethod]
        public void StringEmpty_Title()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.Title,
                "Title is not string.Empty.");

            card.Title = null;

            Assert.IsEmpty(
                card.Title,
                "Title is not string.Empty after being assigned null.");

        }

        #endregion

        #region [ StringEmpty_UniqueId ]

        [TestMethod]
        public void StringEmpty_UniqueId()
        {

            vCard card = new vCard();

            Assert.IsEmpty(
                card.UniqueId,
                "UniqueId is not string.Empty.");

            card.UniqueId = null;

            Assert.IsEmpty(
                card.UniqueId,
                "UniqueId is not string.Empty after being assigned null.");

        }

        #endregion

        public void Dispose() { //driver.Dispose();  
        }
    }
}
