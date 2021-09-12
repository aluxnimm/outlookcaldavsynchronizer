using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thought.vCards;
using Assert = NUnit.Framework.Assert;

namespace Tests
{
    [TestClass]
    public sealed class vCardPhoneTests : IDisposable
    {
        #region [ EmptyString_FullNumber ]

        [TestMethod]
        public void EmptyString_FullNumber()
        {
            vCardPhone phone = new vCardPhone();

            Assert.IsEmpty(
                phone.FullNumber,
                "The full number should default to String.Empty.");

            phone.FullNumber = null;

            Assert.IsEmpty(
                phone.FullNumber,
                "The FullNumber property should return String.Empty instead of null.");
        }

        #endregion

        // The next tests perform a read/write operation against
        // each property of the vCardPhone class.  These tests
        // are primarily intended to catch accidential recursion.

        #region [ ReadWriteProperty_FullNumber ]

        [TestMethod]
        public void ReadWriteProperty_FullNumber()
        {
            // Make sure .FullNumber reads/writes

            vCardPhone phone = new vCardPhone();
            phone.FullNumber = "800-929-5805";
            Assert.AreEqual(
                "800-929-5805",
                phone.FullNumber,
                "The FullNumber property is not working.");
        }

        #endregion

        #region [ ReadWriteProperty_IsBBS ]

        [TestMethod]
        public void ReadWriteProperty_IsBBS()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsBBS = true;
            Assert.IsTrue(phone.IsBBS, "IsBBS should have been set to true.");

            phone.IsBBS = false;
            Assert.IsFalse(phone.IsBBS, "IsBBS should have been set to false.");
        }

        #endregion

        #region [ ReadWriteProperty_IsCar ]

        [TestMethod]
        public void ReadWriteProperty_IsCar()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsCar = true;
            Assert.IsTrue(phone.IsCar, "IsCar should have been set to true.");

            phone.IsCar = false;
            Assert.IsFalse(phone.IsCar, "IsCar should have been set to false.");
        }

        #endregion

        #region [ ReadWriteProperty_IsCellular ]

        [TestMethod]
        public void ReadWriteProperty_IsCellular()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsCellular = true;
            Assert.IsTrue(phone.IsCellular, "IsCellular should have been set to true.");

            phone.IsCellular = false;
            Assert.IsFalse(phone.IsCellular, "IsCellular should have been set to false.");
        }

        #endregion

        #region [ ReadWriteProperty_IsFax ]

        [TestMethod]
        public void ReadWriteProperty_IsFax()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsFax = true;
            Assert.IsTrue(phone.IsFax, "IsFax should have been set to true.");

            phone.IsFax = false;
            Assert.IsFalse(phone.IsFax, "IsFax should have been set to false.");
        }

        #endregion

        #region [ ReadWriteProperty_IsHome ]

        [TestMethod]
        public void ReadWriteProperty_IsHome()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsHome = true;
            Assert.IsTrue(phone.IsHome, "IsHome should have been set to true.");

            phone.IsHome = false;
            Assert.IsFalse(phone.IsHome, "IsHome should have been set to false.");
        }

        #endregion

        #region [ ReadWriteProperty_IsISDN ]

        [TestMethod]
        public void ReadWriteProperty_IsISDN()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsISDN = true;
            Assert.IsTrue(phone.IsISDN, "IsISDN should have been set to true.");

            phone.IsISDN = false;
            Assert.IsFalse(phone.IsISDN, "IsISDN should have been set to false.");
        }

        #endregion

        #region [ ReadWriteProperty_IsMessagingService ]

        [TestMethod]
        public void ReadWriteProperty_IsMessagingService()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsMessagingService = true;
            Assert.IsTrue(phone.IsMessagingService, "IsMessagingService should have been set to true.");

            phone.IsMessagingService = false;
            Assert.IsFalse(phone.IsMessagingService, "IsMessagingService should have been set to false.");
        }

        #endregion

        #region [ ReadWriteProperty_IsModem ]

        [TestMethod]
        public void ReadWriteProperty_IsModem()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsModem = true;
            Assert.IsTrue(phone.IsModem, "IsModem should have been set to true.");

            phone.IsModem = false;
            Assert.IsFalse(phone.IsModem, "IsModem should have been set to false.");
        }

        #endregion

        #region [ ReadWriteProperty_IsPager ]

        [TestMethod]
        public void ReadWriteProperty_IsPager()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsPager = true;
            Assert.IsTrue(phone.IsPager, "IsPager should have been set to true.");

            phone.IsPager = false;
            Assert.IsFalse(phone.IsPager, "IsPager should have been set to false.");
        }

        #endregion

        #region [ ReadWriteProperty_IsPreferred ]

        [TestMethod]
        public void ReadWriteProperty_IsPreferred()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsPreferred = true;
            Assert.IsTrue(phone.IsPreferred, "IsPreferred should have been set to true.");

            phone.IsPreferred = false;
            Assert.IsFalse(phone.IsPreferred, "IsPreferred should have been set to false.");
        }

        #endregion

        #region [ ReadWriteProperty_IsVideo ]

        [TestMethod]
        public void ReadWriteProperty_IsVideo()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsVideo = true;
            Assert.IsTrue(phone.IsVideo, "IsVideo should have been set to true.");

            phone.IsVideo = false;
            Assert.IsFalse(phone.IsVideo, "IsVideo should have been set to false.");
        }

        #endregion

        #region [ ReadWriteProperty_IsVoice ]

        [TestMethod]
        public void ReadWriteProperty_IsVoice()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsVoice = true;
            Assert.IsTrue(phone.IsVoice, "IsVoice should have been set to true.");

            phone.IsVoice = false;
            Assert.IsFalse(phone.IsVoice, "IsVoice should have been set to false.");
        }

        #endregion

        #region [ ReadWriteProperty_IsWork ]

        [TestMethod]
        public void ReadWriteProperty_IsWork()
        {
            vCardPhone phone = new vCardPhone();

            phone.IsWork = true;
            Assert.IsTrue(phone.IsWork, "IsWork should have been set to true.");

            phone.IsWork = false;
            Assert.IsFalse(phone.IsWork, "IsWork should have been set to false.");
        }

        #endregion

        public void Dispose()
        {
            // driver.Dispose(); 
        }
    }
}