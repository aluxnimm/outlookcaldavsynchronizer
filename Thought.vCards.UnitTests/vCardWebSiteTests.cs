
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thought.vCards;
using Assert = NUnit.Framework.Assert;

namespace Tests
{
    [TestClass]
    public sealed class vCardWebSiteTests : IDisposable
    {

        private const string TestUrl = "http://www.thoughtproject.com";

        #region [ Constructor ]

        [TestMethod]
        public void Constructor()
        {

            // Tests for default values when a web site
            // object is created without parameters.

            vCardWebsite site = new vCardWebsite();

            Assert.AreEqual(
                site.WebsiteType,
                vCardWebsiteTypes.Default,
                "The site type should be the default.");

            Assert.IsEmpty(
                site.Url,
                "The Url should be String.Empty because it was not initialized.");

        }

        #endregion

        #region [ Constructor_Url ]

        [TestMethod]
        public void Constructor_Url()
        {

            // Tests for values when a web site is created
            // with a URL.

            vCardWebsite site = new vCardWebsite(TestUrl);

            Assert.AreEqual(
                TestUrl,
                site.Url,
                "The URL was not assigned properly by the constructor.");

            Assert.AreEqual(
                vCardWebsiteTypes.Default,
                site.WebsiteType,
                "The site type should be default because it was not specified.");

        }

        #endregion

        #region [ Constructor_NullUrl ]

        [TestMethod]
        public void Constructor_NullUrl()
        {
            vCardWebsite site = new vCardWebsite((string)null);

            Assert.IsEmpty(
                site.Url,
                "The Url should be String.Empty even when initialized to null.");

        }

        #endregion

        #region [ Constructor_Url_WebSiteType ]

        [TestMethod]
        public void Constructor_Url_WebSiteType()
        {

            vCardWebsite site = new vCardWebsite(
                TestUrl, vCardWebsiteTypes.Personal);

            Assert.AreEqual(
                TestUrl,
                site.Url,
                "The URL was not assigned properly by the constructor.");

            Assert.AreEqual(
                vCardWebsiteTypes.Personal,
                site.WebsiteType,
                "The site type should have been set.");

        }

        #endregion

        #region [ ReadWriteProperty_IsPersonalSite ]

        [TestMethod]
        public void ReadWriteProperty_IsPersonalSite()
        {

            vCardWebsite site = new vCardWebsite();

            site.IsPersonalSite = true;
            Assert.IsTrue(
                site.IsPersonalSite,
                "The IsPersonSite property should be true.");

            site.IsPersonalSite = false;
            Assert.IsFalse(
                site.IsPersonalSite,
                "The IsPersonSite property should be false.");

        }

        #endregion

        #region [ ReadWriteProperty_IsWorkSite ]

        [TestMethod]
        public void ReadWriteProperty_IsWorkSite()
        {

            vCardWebsite site = new vCardWebsite();

            site.IsWorkSite = true;
            Assert.IsTrue(
                site.IsWorkSite,
                "The IsWorkSite property should be true.");

            site.IsWorkSite = false;
            Assert.IsFalse(
                site.IsWorkSite,
                "The IsWorkSite property should be false.");

        }

        #endregion

        #region [ ReadWriteProperty_WebSiteType ]

        [TestMethod]
        public void ReadWriteProperty_WebSiteType()
        {

            vCardWebsite site = new vCardWebsite();

            site.WebsiteType = vCardWebsiteTypes.Personal;
            
            Assert.AreEqual(
                vCardWebsiteTypes.Personal,
                site.WebsiteType,
                "The web site type should be set to Personal.");

            Assert.IsTrue(
                site.IsPersonalSite,
                "The IsPersonalSite property should be true.");

            Assert.IsFalse(
                site.IsWorkSite,
                "The IsWorkSite property should be false.");

            site.WebsiteType = vCardWebsiteTypes.Work;

            Assert.AreEqual(
                vCardWebsiteTypes.Work,
                site.WebsiteType,
                "The web site type should be set to Work.");

            Assert.IsFalse(
                site.IsPersonalSite,
                "The IsPersonalSite property should be false.");

            Assert.IsTrue(
                site.IsWorkSite,
                "The IsWorkSite property should be true.");

            // Now set both types.

            site.WebsiteType = vCardWebsiteTypes.Personal | vCardWebsiteTypes.Work;

            Assert.IsTrue(
                site.IsPersonalSite,
                "The IsPersonalSite property should also be true.");

            Assert.IsTrue(
                site.IsWorkSite,
                "The IsWorkSite property should also be true.");

            Assert.AreEqual(
                vCardWebsiteTypes.Personal | vCardWebsiteTypes.Work,
                site.WebsiteType,
                "The WebSiteType property is not working for multiple values.");

        }

        #endregion

        #region [ ReadWriteProperty_Url ]

        [TestMethod]
        public void ReadWriteProperty_Url()
        {

            vCardWebsite site = new vCardWebsite();

            site.Url = TestUrl;
            Assert.AreEqual(
                TestUrl,
                site.Url,
                "The Url property is not working.");

        }

        #endregion

        public void Dispose() { //driver.Dispose(); 
        }
    }
}
