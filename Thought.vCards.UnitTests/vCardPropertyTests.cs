using System;
using Thought.vCards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = NUnit.Framework.Assert;

namespace Tests
{
    /// <summary>
    ///     Unit tests of the vCardProperty class.
    /// </summary>
    [TestClass]
    public sealed class vCardPropertyTests : IDisposable
    {
        // The next tests ensure the various constructors initialize
        // the class properly (e.g. each constructor should create the
        // subproperties collection and initialize any specified fields).

        #region [ Constructor ]

        [TestMethod]
        public void Constructor()
        {
            vCardProperty property = new vCardProperty();

            Assert.IsNotNull(
                property.Subproperties,
                "The subproperties collection was not created.");

            Assert.IsEmpty(
                property.Subproperties,
                "The subproperties collection should be empty.");

            Assert.IsNull(
                property.Value,
                "The value should be null.");
        }

        #endregion

        #region [ Constructor_Name ]

        [TestMethod]
        public void Constructor_Name()
        {
            vCardProperty property = new vCardProperty("NAME");

            Assert.AreEqual(
                "NAME",
                property.Name,
                "The name is incorrect.");

            Assert.IsNull(
                property.Value,
                "The value should be null.");

            Assert.IsNotNull(
                property.Subproperties,
                "The subproperties collection was not created.");

            Assert.IsEmpty(
                property.Subproperties,
                "The subproperties collection should be empty.");
        }

        #endregion

        #region [ Constructor_Name_Value ]

        [TestMethod]
        public void Constructor_Name_Value()
        {
            // This function tests a constructor that accepts
            // a name and initial value.

            vCardProperty property = new vCardProperty("NAME", "VALUE");

            Assert.AreEqual(
                "NAME",
                property.Name,
                "The name is incorrect.");

            Assert.AreEqual(
                "VALUE",
                property.Value,
                "The value is incorrect.");

            Assert.IsNotNull(
                property.Subproperties,
                "The subproperties collection was not created.");

            Assert.IsEmpty(
                property.Subproperties,
                "The subproperties collection should be empty.");
        }

        #endregion

        public void Dispose()
        {
            // driver.Dispose();
        }
    }
}