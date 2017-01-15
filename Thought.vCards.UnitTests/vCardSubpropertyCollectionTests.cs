
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thought.vCards;

namespace Tests
{

    [TestClass]
    public sealed class vCardSubpropertyCollectionTests : IDisposable
    {

        #region [ Add_Name_Empty ]

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Add_Name_Empty()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.Add(string.Empty);

        }

        #endregion

        #region [ Add_Name_Null ]

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Add_Name_Null()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.Add((string)null);

        }

        #endregion

        #region [ AddOrUpdate_NewNameValue ]

        [TestMethod]
        public void AddOrUpdate_NewNameValue()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.AddOrUpdate("NAME", "VALUE");

            Assert.AreEqual(
                1,
                subs.Count,
                "Only one property should be in the collection.");

            Assert.AreEqual(
                "NAME",
                subs[0].Name,
                "The subproperty does not have the correct name.");

            Assert.AreEqual(
                "VALUE",
                subs[0].Value,
                "The subproperty does not have the correct value.");

        }

        #endregion

        #region [ AddOrUpdate_UpdatedValue ]

        [TestMethod]
        public void AddOrUpdate_UpdatedValue()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.AddOrUpdate("NAME", "VALUE");

            Assert.AreEqual(
                1,
                subs.Count,
                "Only one property should be in the collection.");

            Assert.AreEqual(
                "NAME",
                subs[0].Name,
                "The subproperty does not have the correct name.");

            Assert.AreEqual(
                "VALUE",
                subs[0].Value,
                "The subproperty does not have the correct value.");

            subs.AddOrUpdate("NAME", "VALUE2");

            Assert.AreEqual(
                1,
                subs.Count,
                "Only one property should be in the collection.");

            Assert.AreEqual(
                "NAME",
                subs[0].Name,
                "The subproperty does not have the correct name.");

            Assert.AreEqual(
                "VALUE2",
                subs[0].Value,
                "The subproperty does not have the updated value.");

        }

        #endregion

        #region [ AddOrUpdate_UpdatedValueToNull ]

        [TestMethod]
        public void AddOrUpdate_UpdatedValueToNull()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.AddOrUpdate("NAME", "VALUE");

            Assert.AreEqual(
                1,
                subs.Count,
                "Only one property should be in the collection.");

            Assert.AreEqual(
                "NAME",
                subs[0].Name,
                "The subproperty does not have the correct name.");

            Assert.AreEqual(
                "VALUE",
                subs[0].Value,
                "The subproperty does not have the correct value.");

            subs.AddOrUpdate("NAME", null);

            Assert.AreEqual(
                1,
                subs.Count,
                "Only one property should be in the collection.");

            Assert.AreEqual(
                "NAME",
                subs[0].Name,
                "The subproperty does not have the correct name.");

            Assert.IsNull(
                subs[0].Value,
                "The updated value should be null.");

        }

        #endregion

        #region [ Contains ]

        [TestMethod]
        public void Contains()
        {

            vCardSubpropertyCollection subs = 
                new vCardSubpropertyCollection();

            subs.Add("NAME");

            Assert.IsTrue(
                subs.Contains("NAME"),
                "The collection should contain the specified subproperty.");

            Assert.IsTrue(
                subs.Contains("namE"),
                "Subproperty names are not case-sensitive.");

            Assert.IsFalse(
                subs.Contains((string)null),
                "The Contains method should not return True for null.");

            Assert.IsFalse(
                subs.Contains(string.Empty),
                "The Contains method should not return True for Empty.");

            Assert.IsFalse(
                subs.Contains("SOMENAME"),
                "There is no subproperty with the specified name.");

        }

        #endregion

        #region [ Contains_Empty ]

        [TestMethod]
        public void Contains_Empty()
        {

            vCardSubpropertyCollection subs = new vCardSubpropertyCollection();

            Assert.IsFalse(
                subs.Contains(string.Empty));

        }

        #endregion

        #region [ Contains_Null ]

        [TestMethod]
        public void Contains_Null()
        {

            vCardSubpropertyCollection subs = new vCardSubpropertyCollection();

            Assert.IsFalse(
                subs.Contains((string)null));

        }

        #endregion

        // The GetValue function is a utility method in the
        // vCardSubpropertyCollection class.  It returns the
        // value of the property with the specified name, or
        // null if no such value exists.
        //
        // The basic version of GetValue accepts the name
        // of the subproperty.  An advanced version of GetValue
        // accepts a list of value names that may appear as
        // subproperties.  See comments before the GetValue_ValueList
        // section below.

        #region [ GetValue_Name_Empty ]

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void GetValue_Name_Empty()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.GetValue(string.Empty);

        }

        #endregion

        #region [ GetValue_Name_Null ]

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void GetValue__Name_Null()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.GetValue((string)null);

        }

        #endregion

        #region [ GetValue_Name_ValueDoesNotExist ]

        [TestMethod]
        public void GetValue_Name_ValueDoesNotExist()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            string value1 = subs.GetValue("NAME");

            Assert.IsNull(
                value1,
                "A null value should have been returned.");

        }

        #endregion

        #region [ GetValue_Name_ValueExists ]

        [TestMethod]
        public void GetValue_Name_ValueExists()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.Add("NAME", "VALUE");

            string value = subs.GetValue("NAME");

            Assert.AreEqual(
                "VALUE",
                value,
                "The value should have been returned.");

        }

        #endregion

        // The vCardSubpropertyCollection provides a convenient
        // method that searches for a subproperty by its name and/or
        // a list of potential values.  This is needed because
        // some vCard subproperty values are written without
        // a name. For example, the ENCODING property is typically
        // used to specify BASE64 or QUOTED-PRINTABLE, e.g.:
        //
        //   NAME;ENCODING=BASE64:VALUE
        //
        // However, older versions of vCard allowed the encoding
        // to be specified with its value only (no ENCODING=).
        //
        //   NAME;BASE64:VALUE
        //
        // The problem is that this forces the developer to create
        // more complicated searching code.  The GetValue function
        // accepts a list of known values that might be written
        // without a subproperty name.

        #region [ GetValue_ValueList_NameDoesNotExist_ListValueExists ]

        [TestMethod]
        public void GetValue_ValueList_NameDoesNotExist_ListValueExists()
        {

            // The GetValue function should work even if
            // the potential value list is null.

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.Add("VALUE");

            string value =
                subs.GetValue("NAME", new string[] { "ABC", "VALUE" });

            Assert.AreEqual(
                "VALUE",
                value,
                "The value should have been returned.");

        }

        #endregion

        #region [ GetValue_ValueList_NameDoesNotExist_NullList ]

        [TestMethod]
        public void GetValue_ValueList_NameDoesNotExist_NullList()
        {

            // The GetValue function should work even if
            // the potential value list is null.

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            string value =
                subs.GetValue("NAME", (string[])null);

            Assert.IsNull(
                value,
                "The value should be null.");

        }

        #endregion

        #region [ GetValue_ValueList_NameExists_NullList ]

        [TestMethod]
        public void GetValue_ValueList_NameExists_NullList()
        {

            // The GetValue function should work even if
            // the potential value list is null.

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.Add("NAME", "VALUE");

            string value =
                subs.GetValue("NAME", (string[])null);

            Assert.AreEqual(
                "VALUE",
                value,
                "The value should have been returned despite the null list.");

        }

        #endregion

        // The IndexOf method returns the collection index
        // of a subproperty with the specified name.

        #region [ IndexOf ]

        [TestMethod]
        public void IndexOf()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.Add("NAME0");
            subs.Add("NAME1");
            subs.Add("NAME2");

            Assert.AreEqual(
                0,
                subs.IndexOf("NAME0"),
                "The subproperty should be at index 0.");

            Assert.AreEqual(
                1,
                subs.IndexOf("NAME1"),
                "The subproperty should be at index 1.");

            Assert.AreEqual(
                2,
                subs.IndexOf("NAME2"),
                "The subproperty should be at index 2.");

        }

        #endregion

        #region [ IndexOf_EmptyName ]

        [TestMethod]
        public void IndexOf_EmptyName()
        {

            vCardSubpropertyCollection subs = new vCardSubpropertyCollection();

            // IndexOf should not raise an exception for an empty string.

            Assert.AreEqual(
                -1,
                subs.IndexOf(string.Empty));
        }

        #endregion

        #region [ IndexOf_MissingValueInEmptyCollection ]

        [TestMethod]
        public void IndexOf_MissingValueInEmptyCollection()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            // This test ensures that an empty collection
            // does not cause an error (e.g. it catches any
            // code that assumes the collection to be non-empty).

            Assert.AreEqual(
                -1,
                subs.IndexOf("NAME"));

        }

        #endregion

        #region [ IndexOf_MissingValueInPopulatedCollection ]

        [TestMethod]
        public void IndexOf_MissingValueInPopulatedCollection()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.Add("NAME1");
            subs.Add("NAME2");
            subs.Add("NAME3");

            Assert.AreEqual(
                -1,
                subs.IndexOf("NAME"));

        }

        #endregion

        #region [ IndexOf_Null ]

        [TestMethod]
        public void IndexOf_Null()
        {

            vCardSubpropertyCollection subs = new vCardSubpropertyCollection();

            // The IndexOf method should not raise an exception
            // for a null string.  It should return -1 to indicate
            // the name was not located.

            Assert.AreEqual(
                -1,
                subs.IndexOf((string)null));
        }

        #endregion

        // The IndexOfAny function returns the collection 
        // index of a subproperty with any of the specified
        // names.  The first matching subproperty is returned.

        #region [ IndexOfAny_NoMatches_EmptyCollection ]

        [TestMethod]
        public void IndexOfAny_NoMatches_EmptyCollection()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            int index = subs.IndexOfAny(
                new string[] { "FIND1", "FIND2", "FIND3" });

            Assert.AreEqual(
                -1,
                index,
                "No matches should have been found.");

        }

        #endregion

        #region [ IndexOfAny_NoMatches_PopulatedCollection ]

        [TestMethod]
        public void IndexOfAny_NoMatches_PopulatedCollection()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.Add("NAME1");
            subs.Add("NAME2");
            subs.Add("NAME3");

            int index = subs.IndexOfAny(
                new string[] { "FIND1", "FIND2", "FIND3" });

            Assert.AreEqual(
                -1,
                index,
                "No matches should have been found.");

        }

        #endregion

        #region [ IndexOfAny_OneMatch ]

        [TestMethod]
        public void IndexOfAny_OneMatch()
        {

            vCardSubpropertyCollection subs =
                new vCardSubpropertyCollection();

            subs.Add("NAME0");
            subs.Add("NAME1");
            subs.Add("NAME2");

            int index = subs.IndexOfAny(
                new string[] { "FIND0", "NAME1", "FIND2" });

            Assert.AreEqual(
                1,
                index);

        }

        #endregion

        public void Dispose() {//  driver.Dispose(); 
        }
    }
}
