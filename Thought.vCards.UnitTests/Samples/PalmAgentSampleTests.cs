
using System;
using System.IO;
using NUnit.Framework;
using Thought.vCards;

namespace Tests.Samples
{

    /* ===================================================================
     * PalmAgent hCard Sampe Output Tests
     * -------------------------------------------------------------------
     * 
     * Overview
     * 
     *     This unit test class parses an output file from the PalmAgent
     *     utility found at http://dev.w3.org/cvsweb/2001/palmagent.
     *     Please note that the author of the utility has no affiliation
     *     with the vCard class library.  Also note that the output
     *     from the PalmAgent has not been verified for exact compliance
     *     with the vCard standards.  Nevertheless the sample provides
     *     some good cards "from the wild".
     * 
     * Samples
     * 
     *     The sample file contains 20 tests that are each checked in
     *     detail by a method in this class.
     * 
     *     Test            Start   End
     *     ----------------------------
     *     _checkCard01    001    009
     *     _checkCard02    010    020
     *     _checkCard03    021    029
     *     _checkCard04    030    038
     *     _checkCard05    039    048
     *     _checkCard06    049    058
     *     _checkCard07    059    067
     *     _checkCard08    068    076
     *     _checkCard09    077    085
     *     _checkCard10    086    095
     *     _checkCard11    096    104
     *     _checkCard12    105    113
     *     _checkCard13    114    122
     *     _checkCard14    123    132
     *     _checkCard15    133    145
     *     _checkCard16    146    154
     *     _checkCard17    155    168
     *     _checkCard18    169    182
     *     _checkCard19    183    198
     *     _checkCard20    199    215
     * 
     */

    [TestFixture]
    public class PalmAgentSampleTests
    {

        #region [ CycleCards ]

        [Test]
        public void CycleCards()
        {

            MemoryStream stream =
                new MemoryStream(SampleCards.PalmAgentSamples);

            StreamReader reader =
                new StreamReader(stream);

            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));
            Helper.CycleStandard(new vCard(reader));

        }

        #endregion

        #region [ ParseCards ]

        [Test]
        public void ParseCards()
        {

            // This example loads each card in hCardSamples.txt
            // and performs a detailed test of each one.  For this
            // reason the file should never be modified because it
            // will cause the tests to break.

            MemoryStream stream =
                new MemoryStream(SampleCards.PalmAgentSamples);

            StreamReader reader =
                new StreamReader(stream);

            _checkCard01(new vCard(reader));
            _checkCard02(new vCard(reader));
            _checkCard03(new vCard(reader));
            _checkCard04(new vCard(reader));
            _checkCard05(new vCard(reader));
            _checkCard06(new vCard(reader));
            _checkCard07(new vCard(reader));
            _checkCard08(new vCard(reader));
            _checkCard09(new vCard(reader));
            _checkCard10(new vCard(reader));
            _checkCard11(new vCard(reader));
            _checkCard12(new vCard(reader));
            _checkCard13(new vCard(reader));
            _checkCard14(new vCard(reader));
            _checkCard15(new vCard(reader));
            _checkCard16(new vCard(reader));
            _checkCard17(new vCard(reader));
            _checkCard18(new vCard(reader));
            _checkCard19(new vCard(reader));
            _checkCard20(new vCard(reader));

        }

        #endregion

        // Each vCard in the sample output is tested in detail.

        #region [ _checkCard01 ]

        private void _checkCard01(vCard card)
        {

            // Lines 1 through 9
            //
            // 001 BEGIN:VCARD
            // 002 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 003 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 004 NAME: hCard test data
            // 005 VERSION:3.0
            // 006 FN:John Doe
            // 007 N:Doe;John;;;;
            // 008 NOTE:normal card
            // 009 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 2 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 3 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME failed on line 4.");

            Assert.AreEqual(
                "John Doe",
                card.FormattedName,
                "FN on line 6 failed.");

            // 007 N:Doe;John;;;;

            Assert.AreEqual(
                "Doe",
                card.FamilyName,
                "N (family name) on line 7 failed.");

            Assert.AreEqual(
                "John",
                card.GivenName,
                "N (given name) on line 7 failed.");

            // 008 NOTE:normal card

            Assert.AreEqual(
                "normal card",
                card.Notes[0].Text,
                "NOTE on line 8 failed.");

        }

        #endregion

        #region [ _checkCard02 ]

        private void _checkCard02(vCard card)
        {

            // 010 BEGIN:VCARD
            // 011 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 012 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 013 NAME: hCard test data
            // 014 VERSION:3.0
            // 015 FN:John Doe
            // 016 N:Doe;John;;;;
            // 017 EMAIL:doe@example
            // 018 EMAIL;TYPE=pref:john.doe@example
            // 019 NOTE:card with 2 email addresses
            // 020 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 11 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 12 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME failed on line 13.");

            Assert.AreEqual(
                "John Doe",
                card.FormattedName,
                "FN on line 15 failed.");

            Assert.AreEqual(
                "Doe",
                card.FamilyName,
                "N (family name) failed on line 16.");

            Assert.AreEqual(
                "John",
                card.GivenName,
                "N (given name) failed on line 16.");

            Assert.AreEqual(
                2,
                card.EmailAddresses.Count,
                "There are two email addresses starting at line 17.");

            Assert.AreEqual(
                "doe@example",
                card.EmailAddresses[0].Address,
                "EMAIL on line 17 failed.");

            Assert.AreEqual(
                "john.doe@example",
                card.EmailAddresses[1].Address,
                "EMAIL (address) on line 18 failed.");


        }

        #endregion

        #region [ _checkCard03 ]

        private void _checkCard03(vCard card)
        {

            // 021 BEGIN:VCARD
            // 022 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 023 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 024 NAME: hCard test data
            // 025 VERSION:3.0
            // 026 FN:John Doe
            // 027 N:Doe;John;;;;
            // 028 NOTE:normal card\, with implied N
            // 029 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 22 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 23 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME failed on line 24.");

            Assert.AreEqual(
                "John Doe",
                card.FormattedName,
                "FN on line 26 failed.");

            Assert.AreEqual(
                "Doe",
                card.FamilyName,
                "N (family name) failed on line 27.");

            Assert.AreEqual(
                "John",
                card.GivenName,
                "N (given name) failed on line 27.");

            Assert.AreEqual(
                "normal card, with implied N",
                card.Notes[0].Text,
                "NOTE on line 28 failed; comma should be unescaped.");

        }

        #endregion

        #region [ _checkCard04 ]

        private void _checkCard04(vCard card)
        {
            
            // 030 BEGIN:VCARD
            // 031 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 032 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 033 NAME: hCard test data
            // 034 VERSION:3.0
            // 035 FN:John Doe
            // 036 N:Doe;John;;;;
            // 037 NOTE:card with an ID
            // 038 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 31 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 32 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME failed on line 33.");

            Assert.AreEqual(
                "John Doe",
                card.FormattedName,
                "FN on line 35 failed.");

            Assert.AreEqual(
                "Doe",
                card.FamilyName,
                "N (family name) failed on line 36.");

            Assert.AreEqual(
                "John",
                card.GivenName,
                "N (given name) failed on line 36.");

            Assert.AreEqual(
                "card with an ID",
                card.Notes[0].Text,
                "NOTE on line 37 failed.");

        }

        #endregion

        #region [ _checkCard05 ]

        private void _checkCard05(vCard card)
        {
            
            // 039 BEGIN:VCARD
            // 040 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 041 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 042 NAME: hCard test data
            // 043 VERSION:3.0
            // 044 FN:John Doe
            // 045 N:Doe;John;;;;
            // 046 NOTE:card with relative link
            // 047 URL:http://dev.w3.org/cvsweb/2001/palmagent/doe-pg
            // 048 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 40 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 41 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME failed on line 42.");

            Assert.AreEqual(
                "John Doe",
                card.FormattedName,
                "FN on line 44 failed.");

            Assert.AreEqual(
                "Doe",
                card.FamilyName,
                "N (family name) failed on line 45.");

            Assert.AreEqual(
                "John",
                card.GivenName,
                "N (given name) failed on line 45.");

            Assert.AreEqual(
                "card with relative link",
                card.Notes[0].Text,
                "NOTE on line 46 failed.");

            Assert.AreEqual(
                1,
                card.Websites.Count,
                "Only one web site expected, starting at line 47.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/doe-pg",
                card.Websites[0].Url,
                "URL on line 47 failed.");


        }

        #endregion

        #region [ _checkCard06 ]

        private void _checkCard06(vCard card)
        {
            
            // 049 BEGIN:VCARD
            // 050 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 051 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 052 NAME: hCard test data
            // 053 VERSION:3.0
            // 054 FN:John Doe
            // 055 N:Doe;John;;;;
            // 056 NICKNAME:Johnny
            // 057 NOTE:card with nickname.
            // 058 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 50 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 51 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME on line 52 failed.");

            Assert.AreEqual(
                "John Doe",
                card.FormattedName,
                "FN on line 54 failed.");

            // 055 N:Doe;John;;;;

            Assert.AreEqual(
                "Doe",
                card.FamilyName,
                "N (family name) on line 55 failed.");

            Assert.AreEqual(
                "John",
                card.GivenName,
                "N (given name) on line 55 failed.");

            // 056 NICKNAME:Johnny

            Assert.AreEqual(
                1,
                card.Nicknames.Count,
                "One nickname expected on line 56.");

            Assert.AreEqual(
                "Johnny",
                card.Nicknames[0],
                "NICKNAME on line 56 failed.");

            Assert.AreEqual(
                "card with nickname.",
                card.Notes[0].Text,
                "NOTE on line 57 failed.");

        }

        #endregion

        #region [ _checkCard07 ]

        private void _checkCard07(vCard card)
        {
            
            // 059 BEGIN:VCARD
            // 060 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 061 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 062 NAME: hCard test data
            // 063 VERSION:3.0
            // 064 FN:John Doe2
            // 065 N:Doe2;John;;;;
            // 066 NOTE:tabs in class fields
            // 067 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 60 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 61 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME failed on line 62.");

            Assert.AreEqual(
                "John Doe2",
                card.FormattedName,
                "FN on line 64 failed.");

            Assert.AreEqual(
                "Doe2",
                card.FamilyName,
                "N (family name) failed on line 65.");

            Assert.AreEqual(
                "John",
                card.GivenName,
                "N (given name) failed on line 65.");

            Assert.AreEqual(
                "tabs in class fields",
                card.Notes[0].Text,
                "NOTE on line 66 failed.");

        }

        #endregion

        #region [ _checkCard08 ]

        private void _checkCard08(vCard card)
        {

            // 068 BEGIN:VCARD
            // 069 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 070 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 071 NAME: hCard test data
            // 072 VERSION:3.0
            // 073 FN:John Doe3
            // 074 N:Doe3;John;;;;
            // 075 NOTE:LFs in class fields
            // 076 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 69 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 70 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME failed on line 71.");

            Assert.AreEqual(
                "John Doe3",
                card.FormattedName,
                "FN on line 73 failed.");

            Assert.AreEqual(
                "Doe3",
                card.FamilyName,
                "N (family name) failed on line 74.");

            Assert.AreEqual(
                "John",
                card.GivenName,
                "N (given name) failed on line 74.");

            Assert.AreEqual(
                "LFs in class fields",
                card.Notes[0].Text,
                "NOTE on line 76 failed.");

        }

        #endregion

        #region [ _checkCard09 ]

        private void _checkCard09(vCard card)
        {

            // 077 BEGIN:VCARD
            // 078 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 079 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 080 NAME: hCard test data
            // 081 VERSION:3.0
            // 082 FN:John Doe3
            // 083 N:Doe3;John;;;;
            // 084 NOTE:CRs in class fields
            // 085 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 78 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 79 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME failed on line 80.");

            Assert.AreEqual(
                "John Doe3",
                card.FormattedName,
                "FN on line 82 failed.");

            Assert.AreEqual(
                "Doe3",
                card.FamilyName,
                "N (family name) failed on line 83.");

            Assert.AreEqual(
                "John",
                card.GivenName,
                "N (given name) failed on line 83.");

            Assert.AreEqual(
                "CRs in class fields",
                card.Notes[0].Text,
                "NOTE on line 84 failed.");

        }

        #endregion

        #region [ _checkCard10 ]

        private void _checkCard10(vCard card)
        {
            
            // 086 BEGIN:VCARD
            // 087 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 088 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 089 NAME: hCard test data
            // 090 VERSION:3.0
            // 091 FN:Joe Name2
            // 092 N:Name2;Joe;;;;
            // 093 NOTE:which name counts?
            // 094 URL:http://example/pg
            // 095 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 87 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 88 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME failed on line 89.");

            Assert.AreEqual(
                "Joe Name2",
                card.FormattedName,
                "FN on line 91 failed.");

            Assert.AreEqual(
                "Name2",
                card.FamilyName,
                "N (family name) failed on line 92.");

            Assert.AreEqual(
                "Joe",
                card.GivenName,
                "N (given name) failed on line 92.");

            Assert.AreEqual(
                "which name counts?",
                card.Notes[0].Text,
                "NOTE on line 93 failed.");

            Assert.AreEqual(
                1,
                card.Websites.Count,
                "Only one web site expected at line 94.");

            Assert.AreEqual(
                "http://example/pg",
                card.Websites[0].Url,
                "URL failed on line 94.");


        }

        #endregion

        #region [ _checkCard11 ]

        private void _checkCard11(vCard card)
        {

            // 096 BEGIN:VCARD
            // 097 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 098 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 099 NAME: hCard test data
            // 100 VERSION:3.0
            // 101 FN;LANGUAGE=es:John Doe4
            // 102 N;LANGUAGE=es:Doe4;John;;;;
            // 103 NOTE;LANGUAGE=es:lang dominating vcard
            // 104 END:VCARD


            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 97 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 98 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME failed on line 99.");

            Assert.AreEqual(
                "John Doe4",
                card.FormattedName,
                "FN on line 101 failed.");

            Assert.AreEqual(
                "Doe4",
                card.FamilyName,
                "N (family name) failed on line 102.");

            Assert.AreEqual(
                "John",
                card.GivenName,
                "N (given name) failed on line 102.");

            Assert.AreEqual(
                "lang dominating vcard",
                card.Notes[0].Text,
                "NOTE on line 103 failed.");

            Assert.AreEqual(
                "es",
                card.Notes[0].Language,
                "NOTE;language=es failed on line 103.");
        }

        #endregion

        #region [ _checkCard12 ]

        private void _checkCard12(vCard card)
        {

            // 105 BEGIN:VCARD
            // 106 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 107 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 108 NAME: hCard test data
            // 109 VERSION:3.0
            // 110 FN;LANGUAGE=es:John Doe4
            // 111 N;LANGUAGE=es:Doe4;John;;;;
            // 112 NOTE;LANGUAGE=es:2langs dominating vcard
            // 113 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 106 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 107 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME on line 108 failed.");

            Assert.AreEqual(
                "John Doe4",
                card.FormattedName,
                "FN on line 110 failed.");

            Assert.AreEqual(
                "Doe4",
                card.FamilyName,
                "N (family name) failed on line 111.");

            Assert.AreEqual(
                "John",
                card.GivenName,
                "N (given name) failed on line 111.");

            Assert.AreEqual(
                "2langs dominating vcard",
                card.Notes[0].Text,
                "NOTE failed on line 112.");

            Assert.AreEqual(
                "es",
                card.Notes[0].Language,
                "NOTE;language=es failed on line 112.");

        }

        #endregion

        #region [ _checkCard13 ]

        private void _checkCard13(vCard card)
        {
            
            // 114 BEGIN:VCARD
            // 115 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 116 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 117 NAME: hCard test data
            // 118 VERSION:3.0
            // 119 FN;LANGUAGE=de:John Doe5
            // 120 N;LANGUAGE=de:Doe5;John;;;;
            // 121 NOTE:xml:lang on name elt
            // 122 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 115 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 116 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME on line 117 failed.");

            Assert.AreEqual(
                "John Doe5",
                card.FormattedName,
                "FN on line 119 failed.");

            Assert.AreEqual(
                "Doe5",
                card.FamilyName,
                "N (family name) on line 120 failed.");

            Assert.AreEqual(
                "John",
                card.GivenName,
                "N (given name) on line 120 failed.");

            Assert.AreEqual(
                "xml:lang on name elt",
                card.Notes[0].Text,
                "NOTE on line 121 failed.");

        }

        #endregion

        #region [ _checkCard14 ]

        private void _checkCard14(vCard card)
        {

            // 123 BEGIN:VCARD
            // 124 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 125 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 126 NAME: hCard test data
            // 127 VERSION:3.0
            // 128 FN:Comma Photo
            // 129 N:Photo;Comma;;;;
            // 130 PHOTO;VALUE=uri:http://example/uri\,with\,commas
            // 131 NOTE:comma in photo URI
            // 132 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 124 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 125 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME on line 126 failed.");

            Assert.AreEqual(
                "Comma Photo",
                card.FormattedName,
                "FN on line 128 failed.");

            Assert.AreEqual(
                "Photo",
                card.FamilyName,
                "N (family name) on line 129 failed.");

            // The commas should be escaped out of the URL.

            Assert.AreEqual(
                "http://example/uri,with,commas",
                card.Photos[0].Url.ToString(),
                "PHOTO failed on line 130.");

            Assert.AreEqual(
                "Comma",
                card.GivenName,
                "N (given name) on line 129 failed.");

            Assert.AreEqual(
                "comma in photo URI",
                card.Notes[0].Text,
                "NOTE on line 131 failed.");

        }

        #endregion

        #region [ _checkCard15 ]

        private void _checkCard15(vCard card)
        {
            
            // 133 BEGIN:VCARD
            // 134 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 135 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 136 NAME: hCard test data
            // 137 VERSION:3.0
            // 138 FN:Dan Connolly
            // 139 N:Connolly;Dan;;;;
            // 140 PHOTO;VALUE=uri:http://www.w3.org/People/Connolly/9704/dan_c_thumb.jpg
            // 141 ADR:;;200 Tech Square;Cambridge;MA;02139;;
            // 142 TEL:555-1212
            // 143 ORG:W3C/MIT
            // 144 URL:http://www.w3.org/
            // 145 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 134 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 135 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME on line 136 failed.");

            Assert.AreEqual(
                "Dan Connolly",
                card.FormattedName,
                "FN on line 138 failed.");

            Assert.AreEqual(
                "Connolly",
                card.FamilyName,
                "N (family name) on line 139 failed.");

            Assert.AreEqual(
                "Dan",
                card.GivenName,
                "N (given name) on line 139 failed.");

            Assert.AreEqual(
                "http://www.w3.org/People/Connolly/9704/dan_c_thumb.jpg",
                card.Photos[0].Url.ToString(),
                "PHOTO on line 140 failed.");

            // Note: this sample vCard appears to have a spurious
            // semicolon at the end.  This should be igored by
            // the reader.
            //
            //   ;;200 Tech Square;Cambridge;MA;02139;;
            //

            Assert.AreEqual(
                1,
                card.DeliveryAddresses.Count,
                "One (1) address expected starting on line 141.");

            Assert.AreEqual(
                "200 Tech Square",
                card.DeliveryAddresses[0].Street,
                "ADR (street) failed on line 141.");

            Assert.AreEqual(
                "Cambridge",
                card.DeliveryAddresses[0].City,
                "ADR (street) failed on line 141.");

            Assert.AreEqual(
                "MA",
                card.DeliveryAddresses[0].Region,
                "ADR (region) failed on line 141.");

            Assert.AreEqual(
                "02139",
                card.DeliveryAddresses[0].PostalCode,
                "ADR (postal code) failed on line 141.");

            Assert.AreEqual(
                "555-1212",
                card.Phones[0].FullNumber,
                "TEL on line 142 failed.");

            Assert.AreEqual(
                "W3C/MIT",
                card.Organization,
                "ORG on line 143 failed.");

            Assert.AreEqual(
                "http://www.w3.org/",
                card.Websites[0].Url,
                "URL on line 144 failed.");

        }

        #endregion

        #region [ _checkCard16 ]

        private void _checkCard16(vCard card)
        {
            
            // 146 BEGIN:VCARD
            // 147 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 148 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 149 NAME: hCard test data
            // 150 VERSION:3.0
            // 151 FN:brian suda
            // 152 N:suda;brian;;;;
            // 153 URL:http://suda.co.uk
            // 154 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 147 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 148 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME on line 149 failed.");

            Assert.AreEqual(
                "brian suda",
                card.FormattedName,
                "FN on line 151 failed.");

            Assert.AreEqual(
                "suda",
                card.FamilyName,
                "N (family name) on line 152 failed.");

            Assert.AreEqual(
                "brian",
                card.GivenName,
                "N (given name) on line 152 failed.");

            Assert.AreEqual(
                "http://suda.co.uk",
                card.Websites[0].Url,
                "URL on line 153 failed.");

        }

        #endregion

        #region [ _checkCard17 ]

        private void _checkCard17(vCard card)
        {
            
            // 155 BEGIN:VCARD
            // 156 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 157 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 158 NAME: hCard test data
            // 159 VERSION:3.0
            // 160 FN:Julian F. Reschke
            // 161 N:;;;;;
            // 162 ADR:;;;;;;;
            // 163 TEL;TYPE=voice:+49 251 2807760
            // 164 TEL;TYPE=fax:+49 251 2807761
            // 165 EMAIL:julian.reschke@greenbytes.de
            // 166 ORG:greenbytes GmbH
            // 167 URL:http://greenbytes.de/tech/webdav/
            // 168 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 156 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 157 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME on line 158 failed.");

            Assert.AreEqual(
                "Julian F. Reschke",
                card.FormattedName,
                "FN on line 160 failed.");

            // The ADR property was completely blank.  The library
            // should not have created a dummy address.

            Assert.IsEmpty(card.DeliveryAddresses,
                "ADR on line 162 should not have been added.");

            Assert.AreEqual(
                2,
                card.Phones.Count,
                "Two (2) phones expected at line 163.");

            // 163 TEL;TYPE=voice:+49 251 2807760
            // 164 TEL;TYPE=fax:+49 251 2807761

            Assert.AreEqual(
                "+49 251 2807760",
                card.Phones[0].FullNumber,
                "TEL at line 163 failed.");

            Assert.IsTrue(
                card.Phones[0].IsVoice,
                "TEL at line 163 is expected to be voice.");

            Assert.AreEqual(
                "+49 251 2807761",
                card.Phones[1].FullNumber,
                "TEL at line 164 failed.");

            Assert.IsTrue(
                card.Phones[1].IsFax,
                "TEL at line 164 is expected to be a fax.");

            Assert.IsFalse(
                card.Phones[1].IsVoice,
                "TEL at line 164 should not be marked as voice.");


            // 165 EMAIL:julian.reschke@greenbytes.de
            // 166 ORG:greenbytes GmbH
            // 167 URL:http://greenbytes.de/tech/webdav/

            Assert.AreEqual(
                1,
                card.EmailAddresses.Count,
                "One (1) email address expected at line 165.");

            Assert.AreEqual(
                "julian.reschke@greenbytes.de",
                card.EmailAddresses[0].Address,
                "EMAIL at line 165 failed.");

            Assert.AreEqual(
                "greenbytes GmbH",
                card.Organization,
                "ORG at line 166 failed.");

            Assert.AreEqual(
                1,
                card.Websites.Count,
                "One (1) web site expected at line 166.");

            Assert.AreEqual(
                "http://greenbytes.de/tech/webdav/",
                card.Websites[0].Url,
                "URL at line 167 failed.");

        }

        #endregion

        #region [ _checkCard18 ]

        private void _checkCard18(vCard card)
        {
            
            // 169 BEGIN:VCARD
            // 170 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 171 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 172 NAME: hCard test data
            // 173 VERSION:3.0
            // 174 FN:Julian F. Reschke
            // 175 N:Reschke;Julian F.;;;;
            // 176 ADR:;;Salzmannstrasse 152;Muenster;NW;;Germany;
            // 177 TEL;TYPE=voice:+49 251 2807760
            // 178 TEL;TYPE=fax:+49 251 2807761
            // 179 EMAIL:julian.reschke@greenbytes.de
            // 180 ORG:greenbytes GmbH
            // 181 URL:http://greenbytes.de/tech/webdav/
            // 182 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 170 failed.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 171 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME on line 172 failed.");

            Assert.AreEqual(
                "Julian F. Reschke",
                card.FormattedName,
                "FN on line 174 failed.");

            // 175 N:Reschke;Julian F.;;;;

            Assert.AreEqual(
                "Reschke",
                card.FamilyName,
                "N (family name) on line 175 failed.");

            Assert.AreEqual(
                "Julian F.",
                card.GivenName,
                "N (given name) on line 175 failed.");

            // 176 ADR:;;Salzmannstrasse 152;Muenster;NW;;Germany;

            Assert.AreEqual(
                1,
                card.DeliveryAddresses.Count,
                "One (1) address expected at line 176.");

            Assert.AreEqual(
                "Salzmannstrasse 152",
                card.DeliveryAddresses[0].Street,
                "ADR (street) on line 176 failed.");

            Assert.AreEqual(
                "Muenster",
                card.DeliveryAddresses[0].City,
                "ADR (city) on line 176 failed.");

            Assert.AreEqual(
                "NW",
                card.DeliveryAddresses[0].Region,
                "ADR (region) on line 176 failed.");

            Assert.AreEqual(
                "Germany",
                card.DeliveryAddresses[0].Country,
                "ADR (country) on line 176 failed.");

            // 177 TEL;TYPE=voice:+49 251 2807760
            // 178 TEL;TYPE=fax:+49 251 2807761

            Assert.AreEqual(
                2,
                card.Phones.Count,
                "Two (2) phones expected on lines 177 and 178.");

            Assert.AreEqual(
                "+49 251 2807760",
                card.Phones[0].FullNumber,
                "TEL on line 177 failed.");

            Assert.IsTrue(
                card.Phones[0].IsVoice,
                "TEL on line 177 failed (should be voice).");

            Assert.AreEqual(
                "+49 251 2807761",
                card.Phones[1].FullNumber,
                "TEL on line 178 failed.");

            Assert.IsTrue(
                card.Phones[1].IsFax,
                "TEL on line 178 failed (should be fax).");

            // 179 EMAIL:julian.reschke@greenbytes.de

            Assert.AreEqual(
                1,
                card.EmailAddresses.Count,
                "One (1) email expected on line 179.");

            Assert.AreEqual(
                "julian.reschke@greenbytes.de",
                card.EmailAddresses[0].Address,
                "EMAIL on line 179 failed.");

            // 180 ORG:greenbytes GmbH

            Assert.AreEqual(
                "greenbytes GmbH",
                card.Organization,
                "ORG on line 180 failed.");

            // 181 URL:http://greenbytes.de/tech/webdav/

            Assert.AreEqual(
                1,
                card.Websites.Count,
                "One web site expected on line 181.");

            Assert.AreEqual(
                "http://greenbytes.de/tech/webdav/",
                card.Websites[0].Url,
                "URL on line 181 failed.");

        }

        #endregion

        #region [ _checkCard19 ]

        private void _checkCard19(vCard card)
        {

            // 183 BEGIN:VCARD
            // 184 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 185 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 186 NAME: hCard test data
            // 187 VERSION:3.0
            // 188 N:;;;Mr.;;
            // 189 NICKNAME:Jim,Jimmy
            // 190 PHOTO;VALUE=uri:http://www.abc.com/pub/photos/jqpublic.gif
            // 191 BDAY:1987-09-27T08:30:00-06:00
            // 192 TEL;TYPE=work,pref,voice,msg:+1-213-555-1234
            // 193 EMAIL:jqpublic@xyz.dom1.com
            // 194 LOGO;VALUE=uri:http://www.abc.com/pub/logos/abccorp.jpg
            // 195 CATEGORIES:INTERNET,IETF,INDUSTRY,INFORMATION TECHNOLOGY
            // 196 NOTE:This fax number is operational 0800 to 1715 EST\\, Mon-Fri.
            // 197 URL:http://www.swbyps.restaurant.french/~chezchic.html
            // 198 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 184 failed.");

            Assert.AreEqual(
                1,
                card.Sources.Count,
                "One source expected at line 185.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 185 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME on line 186 failed.");

            // 188 N:;;;Mr.;;

            Assert.AreEqual(
                "Mr.",
                card.NamePrefix,
                "N (suffix) failed on line 188.");

            // 189 NICKNAME:Jim,Jimmy

            Assert.AreEqual(
                2,
                card.Nicknames.Count,
                "Two nicknames expected on line 189.");

            Assert.AreEqual(
                "Jim",
                card.Nicknames[0],
                "NICKNAME (Jim) failed on line 189.");

            Assert.AreEqual(
                "Jimmy",
                card.Nicknames[1],
                "NICKNAME (Jimmy) failed on line 189.");

            // 190 PHOTO;VALUE=uri:http://www.abc.com/pub/photos/jqpublic.gif

            Assert.AreEqual(
                1,
                card.Photos.Count,
                "One photo expected on line 190.");

            Assert.AreEqual(
                "http://www.abc.com/pub/photos/jqpublic.gif",
                card.Photos[0].Url.ToString(),
                "PHOTO (url) failed on line 190.");

            // 191 BDAY:1987-09-27T08:30:00-06:00

            Assert.IsNotNull(
                card.BirthDate,
                "BDAY on line 191 should not be null.");

            Assert.AreEqual(
                DateTime.Parse("1987-09-27T08:30:00-06:00"),
                card.BirthDate.Value,
                "BDAY on line 191 failed.");

            // 192 TEL;TYPE=work,pref,voice,msg:+1-213-555-1234

            Assert.AreEqual(
                1,
                card.Phones.Count,
                "One TEL expected on line 192.");

            Assert.IsTrue(
                card.Phones[0].IsWork,
                "TEL (work) on line 192 failed.");

            Assert.IsTrue(
                card.Phones[0].IsPreferred,
                "TEL (preferred) on line 192 failed.");

            Assert.IsTrue(
                card.Phones[0].IsVoice,
                "TEL (voice) on line 192 failed.");

            Assert.IsTrue(
                card.Phones[0].IsMessagingService,
                "TEL (messaging) on line 192 failed.");

            Assert.AreEqual(
                "+1-213-555-1234",
                card.Phones[0].FullNumber,
                "TEL value on line 192 failed.");

            // 193 EMAIL:jqpublic@xyz.dom1.com

            Assert.AreEqual(
                1,
                card.EmailAddresses.Count,
                "One email address on line 193 expected.");

            Assert.AreEqual(
                "jqpublic@xyz.dom1.com",
                card.EmailAddresses[0].Address,
                "EMAIL on line 193 failed.");

            // 194 LOGO;VALUE=uri:http://www.abc.com/pub/logos/abccorp.jpg

            // Support?

            // 195 CATEGORIES:INTERNET,IETF,INDUSTRY,INFORMATION TECHNOLOGY

            Assert.AreEqual(
                4,
                card.Categories.Count,
                "Four categories expected on line 195.");

            Assert.AreEqual(
                "INTERNET",
                card.Categories[0],
                "INTERNET category expected on line 195.");

            Assert.AreEqual(
                "IETF",
                card.Categories[1],
                "IETF category expected on line 195.");

            Assert.AreEqual(
                "INDUSTRY",
                card.Categories[2],
                "INDUSTRY category expected on line 195.");

            Assert.AreEqual(
                "INFORMATION TECHNOLOGY",
                card.Categories[3],
                "INFORMATION TECHNOLOGY expected on line 195.");

            // 196 NOTE:This fax number is operational 0800 to 1715 EST\\, Mon-Fri.

            Assert.AreEqual(
                1,
                card.Notes.Count,
                "One note expected on line 196.");

            Assert.AreEqual(
                "This fax number is operational 0800 to 1715 EST\\, Mon-Fri.",
                card.Notes[0].Text,
                "NOTE on line 196 failed.");

            // 197 URL:http://www.swbyps.restaurant.french/~chezchic.html

            Assert.AreEqual(
                1,
                card.Websites.Count,
                "One web site expected on line 197.");

            Assert.AreEqual(
                "http://www.swbyps.restaurant.french/~chezchic.html",
                card.Websites[0].Url,
                "URL on line 197 failed.");

        }

        #endregion

        #region [ _checkCard20 ]

        private void _checkCard20(vCard card)
        {

            // 199 BEGIN:VCARD
            // 200 PRODID:-//connolly.w3.org//palmagent 0.6 (BETA)//EN
            // 201 SOURCE: http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html
            // 202 NAME: hCard test data
            // 203 VERSION:3.0
            // 204 N:Public;;Quinlan;Mr.;Esq.;
            // 205 NICKNAME:Jim,Jimmy
            // 206 PHOTO;VALUE=uri:http://www.abc.com/pub/photos/jqpublic.gif
            // 207 BDAY:1987-09-27T08:30:00-06:00
            // 208 TEL;TYPE=work,pref,voice,msg:+1-213-555-1234
            // 209 EMAIL:jqpublic@xyz.dom1.com
            // 210 TZ:-05:00
            // 211 LOGO;VALUE=uri:http://www.abc.com/pub/logos/abccorp.jpg
            // 212 CATEGORIES:INTERNET,IETF,INDUSTRY,INFORMATION TECHNOLOGY
            // 213 NOTE:This fax number is operational 0800 to 1715 EST\\, Mon-Fri.
            // 214 URL:http://www.swbyps.restaurant.french/~chezchic.html
            // 215 END:VCARD

            Assert.AreEqual(
                "-//connolly.w3.org//palmagent 0.6 (BETA)//EN",
                card.ProductId,
                "PRODID on line 200 failed.");

            Assert.AreEqual(
                1,
                card.Sources.Count,
                "One source expected at line 201.");

            Assert.AreEqual(
                "http://dev.w3.org/cvsweb/2001/palmagent/hcardTest.html",
                card.Sources[0].Uri.ToString(),
                "SOURCE on line 201 failed.");

            Assert.AreEqual(
                "hCard test data",
                card.DisplayName,
                "NAME on line 202 failed.");

            // 204 N:Public;;Quinlan;Mr.;Esq.;

            Assert.AreEqual(
                "Public",
                card.FamilyName,
                "N (family name) on line 204 failed.");

            Assert.AreEqual(
                "Quinlan",
                card.AdditionalNames,
                "N (additional names) on line 204 failed.");

            Assert.AreEqual(
                "Mr.",
                card.NamePrefix,
                "N (prefix) on line 204 failed.");

            Assert.AreEqual(
                "Esq.",
                card.NameSuffix,
                "N (suffix) on line 204 failed.");

            // 205 NICKNAME:Jim,Jimmy

            Assert.AreEqual(
                2,
                card.Nicknames.Count,
                "Two nicknames expected on line 205.");

            Assert.AreEqual(
                "Jim",
                card.Nicknames[0],
                "NICKNAME (Jim) failed on line 205.");

            Assert.AreEqual(
                "Jimmy",
                card.Nicknames[1],
                "NICKNAME (Jimmy) failed on line 205.");

            // 206 PHOTO;VALUE=uri:http://www.abc.com/pub/photos/jqpublic.gif

            Assert.AreEqual(
                1,
                card.Photos.Count,
                "One photo expected on line 206.");

            Assert.AreEqual(
                "http://www.abc.com/pub/photos/jqpublic.gif",
                card.Photos[0].Url.ToString(),
                "PHOTO (url) failed on line 206.");

            // 207 BDAY:1987-09-27T08:30:00-06:00

            Assert.IsNotNull(
                card.BirthDate,
                "BDAY on line 207 should not be null.");

            Assert.AreEqual(
                DateTime.Parse("1987-09-27T08:30:00-06:00"),
                card.BirthDate.Value,
                "BDAY on line 207 failed.");

            // 208 TEL;TYPE=work,pref,voice,msg:+1-213-555-1234

            Assert.AreEqual(
                1,
                card.Phones.Count,
                "One TEL expected on line 208.");

            Assert.IsTrue(
                card.Phones[0].IsWork,
                "TEL (work) on line 208 failed.");

            Assert.IsTrue(
                card.Phones[0].IsPreferred,
                "TEL (preferred) on line 208 failed.");

            Assert.IsTrue(
                card.Phones[0].IsVoice,
                "TEL (voice) on line 208 failed.");

            Assert.IsTrue(
                card.Phones[0].IsMessagingService,
                "TEL (messaging) on line 208 failed.");

            Assert.AreEqual(
                "+1-213-555-1234",
                card.Phones[0].FullNumber,
                "TEL value on line 208 failed.");

            // 209 EMAIL:jqpublic@xyz.dom1.com

            Assert.AreEqual(
                1,
                card.EmailAddresses.Count,
                "One email address on line 209 expected.");

            Assert.AreEqual(
                "jqpublic@xyz.dom1.com",
                card.EmailAddresses[0].Address,
                "EMAIL on line 209 failed.");

            // 210 TZ:-05:00

            Assert.AreEqual(
                "-05:00",
                card.TimeZone,
                "TZ on line 210 failed.");

            // 211 LOGO;VALUE=uri:http://www.abc.com/pub/logos/abccorp.jpg

            // not supported yet

            // 212 CATEGORIES:INTERNET,IETF,INDUSTRY,INFORMATION TECHNOLOGY

            Assert.AreEqual(
                4,
                card.Categories.Count,
                "Four categories expected on line 212.");

            Assert.AreEqual(
                "INTERNET",
                card.Categories[0],
                "INTERNET category expected on line 212.");

            Assert.AreEqual(
                "IETF",
                card.Categories[1],
                "IETF category expected on line 212.");

            Assert.AreEqual(
                "INDUSTRY",
                card.Categories[2],
                "INDUSTRY category expected on line 212.");

            Assert.AreEqual(
                "INFORMATION TECHNOLOGY",
                card.Categories[3],
                "INFORMATION TECHNOLOGY expected on line 212.");

            // 213 NOTE:This fax number is operational 0800 to 1715 EST\\, Mon-Fri.

            Assert.AreEqual(
                1,
                card.Notes.Count,
                "One note expected on line 213.");

            Assert.AreEqual(
                "This fax number is operational 0800 to 1715 EST\\, Mon-Fri.",
                card.Notes[0].Text,
                "NOTE on line 213 failed.");

            // 214 URL:http://www.swbyps.restaurant.french/~chezchic.html

            Assert.AreEqual(
                1,
                card.Websites.Count,
                "One web site expected on line 214.");

            Assert.AreEqual(
                "http://www.swbyps.restaurant.french/~chezchic.html",
                card.Websites[0].Url,
                "URL on line 214 failed.");

        }

        #endregion

    }
}
