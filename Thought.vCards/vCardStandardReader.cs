/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Thought.vCards
{

	/// <summary>
	///     Reads a vCard written in the standard 2.0 or 3.0 text formats.
	///     This is the primary (standard) vCard format used by most
	///     applications.  
	/// </summary>
	/// <seealso cref="vCardReader"/>
	public class vCardStandardReader : vCardReader
	{

		/// <summary>
		///     The DeliveryAddressTypeNames array contains the recognized
		///     TYPE values for an ADR (delivery address).
		/// </summary>
		private readonly string[] DeliveryAddressTypeNames = new string[] {
			"DOM",      // Domestic address
			"INTL",     // International address
			"POSTAL",   // Postal address
			"PARCEL",   // Parcel delivery address
			"HOME",     // Home address
			"WORK",     // Work address
			"PREF" };   // Preferred address


		/// <summary>
		///     The PhoneTypeNames constant defines the recognized
		///     subproperty names that identify the category or
		///     classification of a phone.  The names are used with
		///     the TEL property.
		/// </summary>
		private readonly string[] PhoneTypeNames = new string[] {
			"BBS",
			"CAR",
			"CELL",
			"FAX",
			"HOME",
			"ISDN",
			"MODEM",
			"MSG",
			"PAGER",
			"PREF",
			"VIDEO",
			"VOICE",
			"WORK" };

		/// <summary>
		///     The state of the quoted-printable decoder (private).
		/// </summary>
		/// <remarks>
		///     The <see cref="DecodeQuotedPrintable(string)"/> function
		///     is a utility function that parses a string that
		///     has been encoded with the QUOTED-PRINTABLE format.
		///     The function is implemented as a state-pased parser
		///     where the state is updated after examining each 
		///     character of the input string.  This enumeration
		///     defines the various states of the parser.
		/// </remarks>
		private enum QuotedPrintableState
		{
			None,
			ExpectingHexChar1,
			ExpectingHexChar2,
			ExpectingLineFeed
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="vCardStandardReader"/>.
		/// </summary>
		public vCardStandardReader()
			: base()
		{ }

		#region [ DecodeBase64(string) ]

		/// <summary>
		///     Decodes a string containing BASE64 characters.
		/// </summary>
		/// <param name="value">
		///     A string containing data that has been encoded with
		///     the BASE64 format.
		/// </param>
		/// <returns>
		///     The decoded data as a byte array.
		/// </returns>
		public static byte[] DecodeBase64(string value)
		{

			// Currently the .NET implementation is acceptable.  However,
			// a different algorithm may be used in the future.  For
			// this reason callers should use this function
			// instead of the FromBase64String function in .NET.
			// Performance is not an issue because the runtime engine
			// will inline the code or eliminate the extra call.

			return Convert.FromBase64String(value);
		}

		#endregion

		#region [ DecodeBase64(char[]) ]

		/// <summary>
		///     Converts BASE64 data that has been stored in a 
		///     character array.
		/// </summary>
		/// <param name="value">
		///     A character array containing BASE64 data.
		/// </param>
		/// <returns>
		///     A byte array containing the decoded BASE64 data.
		/// </returns>
		public static byte[] DecodeBase64(char[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			else
			{
				return Convert.FromBase64CharArray(value, 0, value.Length);
			}
		}

		#endregion

		/// <summary>
		/// returns the parsed ItemType for subProperty values like HOME and WORK. If no match is found, this method returns null
		/// </summary>
		/// <param name="keyword"></param>
		/// <returns></returns>
		public static ItemType? DecodeItemType(string keyword)
		{
			if (string.IsNullOrEmpty(keyword))
				return null;

			switch (keyword.ToUpperInvariant())
			{
				case "HOME":
					return ItemType.HOME;

				case "WORK":
					return ItemType.WORK;

				default:
					return null;
			}




		}

		#region [ DecodeEmailAddressType ]

		/// <summary>
		///     Parses the name of an email address type.
		/// </summary>
		/// <param name="keyword">
		///     The email address type keyword found in the vCard file (e.g. AOL or INTERNET).
		/// </param>
		/// <returns>
		///     Null or the decoded <see cref="vCardEmailAddressType"/>.
		/// </returns>
		/// <seealso cref="vCardEmailAddress"/>
		/// <seealso cref="vCardEmailAddressType"/>
		public static vCardEmailAddressType? DecodeEmailAddressType(string keyword)
		{

			if (string.IsNullOrEmpty(keyword))
				return null;

			switch (keyword.ToUpperInvariant())
			{

				case "INTERNET":
					return vCardEmailAddressType.Internet;

				case "AOL":
					return vCardEmailAddressType.AOL;

				case "APPLELINK":
					return vCardEmailAddressType.AppleLink;

				case "ATTMAIL":
					return vCardEmailAddressType.AttMail;

				case "CIS":
					return vCardEmailAddressType.CompuServe;

				case "EWORLD":
					return vCardEmailAddressType.eWorld;

				case "IBMMAIL":
					return vCardEmailAddressType.IBMMail;

				case "MCIMAIL":
					return vCardEmailAddressType.MCIMail;

				case "POWERSHARE":
					return vCardEmailAddressType.PowerShare;

				case "PRODIGY":
					return vCardEmailAddressType.Prodigy;

				case "TLX":
					return vCardEmailAddressType.Telex;

				case "X400":
					return vCardEmailAddressType.X400;

				default:
					return null;

			}


		}

		#endregion

		#region [ DecodeEscaped ]

		/// <summary>
		///     Decodes a string that has been encoded with the standard
		///     vCard escape codes.
		/// </summary>
		/// <param name="value">
		///     A string encoded with vCard escape codes.
		/// </param>
		/// <returns>
		///     The decoded string.
		/// </returns>
		public static string DecodeEscaped(string value)
		{

			if (string.IsNullOrEmpty(value))
				return value;

			StringBuilder builder = new StringBuilder(value.Length);

			int startIndex = 0;

			do
			{

				// Get the index of the next backslash character.
				// This marks the beginning of an escape sequence.

				int nextIndex = value.IndexOf('\\', startIndex);

				if ((nextIndex == -1) || (nextIndex == value.Length - 1))
				{
					// There are no more escape codes, or the backslash
					// is located at the very end of the string.  The
					// characters between the index and the end of the
					// string need to be copied to the output buffer.

					builder.Append(
						value,
						startIndex,
						value.Length - startIndex);

					break;

				}
				else
				{

					// A backslash was located somewhere in the string.
					// The previous statement ensured the backslash is
					// not the very last character, and therefore the
					// following statement is safe.

					char code = value[nextIndex + 1];

					// Any characters between the starting point and
					// the index must be pushed into the buffer.

					builder.Append(
						value,
						startIndex,
						nextIndex - startIndex);

					switch (code)
					{

						case '\\':
						case ',':
						case ';':

							builder.Append(code);
							nextIndex += 2;
							break;

						case 'n':
						case 'N':
							builder.Append('\n');
							nextIndex += 2;
							break;

						case 'r':
						case 'R':
							builder.Append('\r');
							nextIndex += 2;
							break;

						default:
							builder.Append('\\');
							builder.Append(code);
							nextIndex += 2;
							break;

					}


				}

				startIndex = nextIndex;

			} while (startIndex < value.Length);

			return builder.ToString();

		}

		#endregion

		#region [ DecodeHexadecimal ]

		/// <summary>
		///     Converts a single hexadecimal character to
		///     its integer value.
		/// </summary>
		/// <param name="value">
		///     A Unicode character.
		/// </param>
		public static int DecodeHexadecimal(char value)
		{

			if (char.IsDigit(value))
			{
				return Convert.ToInt32(char.GetNumericValue(value));
			}

			// A = ASCII 65
			// F = ASCII 70
			// a = ASCII 97
			// f = ASCII 102

			else if ((value >= 'A') && (value <= 'F'))
			{

				// The character is one of the characters
				// between 'A' (value 65) and 'F' (value 70).
				// The character "A" (hex) is "10" (decimal).

				return Convert.ToInt32(value) - 55;
			}

			else if ((value >= 'a') && (value <= 'f'))
			{

				// The character is one of the characters
				// between 'a' (value 97) and 'f' (value 102).
				// The character "A" or "a" (hex) is "10" (decimal).

				return Convert.ToInt32(value) - 87;

			}
			else

				// The specified character cannot be interpreted
				// as a written hexadecimal character.  Raise an
				// exception.

				throw new ArgumentOutOfRangeException("value");

		}

		#endregion

		#region [ DecodeQuotedPrintable ]
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string DecodeQuotedPrintable(string value)
		{
			return DecodeQuotedPrintable(value, Encoding.Default);
		}

		/// <summary>
		///     Decodes a string that has been encoded in QUOTED-PRINTABLE format.
		/// </summary>
		/// <param name="value">
		///     A string that has been encoded in QUOTED-PRINTABLE.
		/// </param>
		/// <param name="encoding">
		///     charset encoding 
		/// </param>
		/// <returns>
		///     The decoded string.
		/// </returns>
		public static string DecodeQuotedPrintable(string value, Encoding encoding)
		{

			if (string.IsNullOrEmpty(value))
				return value;

			char firstHexChar = '\x0';
			QuotedPrintableState state = QuotedPrintableState.None;

			System.Collections.Generic.List<Char> charList = new System.Collections.Generic.List<Char>();

			foreach (char c in value)
			{

				switch (state)
				{

					case QuotedPrintableState.None:

						// The parser is not expacting any particular
						// type of character.  If the character is an
						// equal sign (=), then this point in the string
						// is the start of a character encoded in hexadecimal
						// format.  There are two hexadecimal characters
						// expected.

						if (c == '=')
						{
							state = QuotedPrintableState.ExpectingHexChar1;
						}
						else
						{
							charList.Add(c);
						}
						break;

					case QuotedPrintableState.ExpectingHexChar1:

						// The parser previously encountered an equal sign.
						// This has two purposes: it marks the beginning of
						// a hexadecimal escape sequence, or it marks a
						// so-called software end-of-line.

						if (IsHexDigit(c))
						{

							// The next character is a hexadecimal character.
							// Therefore the equal sign marks the beginning
							// of an escape sequence.

							firstHexChar = c;
							state = QuotedPrintableState.ExpectingHexChar2;
						}

						else if (c == '\r')
						{

							// The prior equal sign was located immediately
							// before carriage-return.  This indicates a soft
							// line break that is ignored.  The next character
							// is expected to be a line feed.

							state = QuotedPrintableState.ExpectingLineFeed;

						}

						else if (c == '=')
						{

							// Another equal sign was encountered.  This is
							// bad data.  The parser will output this bad
							// character and assume this equal sign marks
							// the beginning of a sequence.

							charList.Add('=');
							state = QuotedPrintableState.ExpectingHexChar1;

						}

						else
						{

							// The character after the equal sign was
							// not a hex digit, a carriage return, or an
							// equal sign.  It is bad data.

							charList.Add('=');
							charList.Add(c);

							state = QuotedPrintableState.None;
						}
						break;

					case QuotedPrintableState.ExpectingHexChar2:

						// The parser previously encountered an equal
						// sign and the first of two hexadecimal
						// characters.  This character is expected to
						// be the second (final) hexadecimal character.

						if (IsHexDigit(c))
						{

							// Each hexadecimal character represents
							// four bits of the encoded ASCII value.
							// The first character was the upper 4 bits.

							int charValue =
								(DecodeHexadecimal(firstHexChar) << 4) +
								DecodeHexadecimal(c);

							charList.Add((char)charValue);

							state = QuotedPrintableState.None;

						}
						else
						{

							// The parser was expecting the second
							// hexadecimal character after the equal sign.
							// Since this is not a hexadecimal character,
							// the partial sequence is dumped to the output
							// and skipped.

							charList.Add('=');
							charList.Add(firstHexChar);
							charList.Add(c);
							state = QuotedPrintableState.None;

						}
						break;

					case QuotedPrintableState.ExpectingLineFeed:

						// Previously the parser encountered an equal sign
						// followed by a carriage-return.  This is an indicator
						// to the decoder that the encoded value contains a 
						// soft line break.  The line break is ignored.
						// Per mime standards, the character following the
						// carriage-return should be a line feed.

						if (c == '\n')
						{
							state = QuotedPrintableState.None;
						}
						else if (c == '=')
						{
							// A line feed was expected but another equal
							// sign was encountered.  Assume the encoder
							// failed to write a line feed.

							state = QuotedPrintableState.ExpectingHexChar1;
						}
						else
						{
							charList.Add(c);
							state = QuotedPrintableState.None;
						}

						break;
				}
			}

			// The parser has examined each character in the input string.
			// In theory (for a correct string), the parser state should be
			// none -- that is, all codes were property terminated.  If not,
			// the partial codes should be flushed to the output.

			switch (state)
			{
				case QuotedPrintableState.ExpectingHexChar1:
					charList.Add('=');
					break;

				case QuotedPrintableState.ExpectingHexChar2:
					charList.Add('=');
					charList.Add(firstHexChar);
					break;

				case QuotedPrintableState.ExpectingLineFeed:
					charList.Add('=');
					charList.Add('\r');
					break;
			}

			var by = new byte[charList.Count];
			for (int i = 0; i < charList.Count; i++)
			{
				by[i] = Convert.ToByte(charList[i]);
			}

			var ret = encoding.GetString(by);

			return ret;

		}

		#endregion

		#region [ IsHexDigit ]

		/// <summary>
		///     Indicates whether the specified character is
		///     a hexadecimal digit.
		/// </summary>
		/// 
		/// <param name="value">
		///     A unicode character
		/// </param>
		public static bool IsHexDigit(char value)
		{

			// First, see if the character is
			// a decimal digit.  All decimal digits
			// are also hexadecimal digits.

			if (char.IsDigit(value))

				return true;

			return
				((value >= 'A') && (value <= 'F')) ||
				((value >= 'a') && (value <= 'f'));

		}

		#endregion

		// The following functions (Parse*) are utility functions
		// that convert string values into their corresponding
		// enumeration values from the class library.  Some misc.
		// parser functions are also present.

		#region [ ParseDate ]

		/// <summary>
		///     Parses a string containing a date/time value.
		/// </summary>
		/// <param name="value">
		///     A string containing a date/time value.
		/// </param>
		/// <returns>
		///     The parsed date, or null if no date could be parsed.
		/// </returns>
		/// <remarks>
		///     Some revision dates, such as those generated by Outlook,
		///     are not directly supported by the .NET DateTime parser.
		///     This function attempts to accomodate the non-standard formats.
		/// </remarks>
		public static DateTime? ParseDate(string value)
		{

			DateTime parsed;
			if (DateTime.TryParseExact(
				  value, // 2006-11-30T18:40:00Z
				  @"yyyy-MM-ddTHH:mm:ss\Z",
				  null,
				  DateTimeStyles.None,
				  out parsed))
			{
				parsed = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
				return parsed;
			}


			if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out parsed))
			{
				parsed = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
				return parsed;
			}


			// Outlook generates a revision date like this:
			//
			//   20061130T234000Z
			//   |   | | || | ++------- Seconds (2 digits)
			//   |   | | || ++--------- Minutes (2 digits)
			//   |   | | |++----------- Hour (2 digits)
			//   |   | | +------------- T (literal)
			//   |   | ++-------------- Day (2 digits)
			//   |   ++---------------- Month (2 digits)             
			//   +--+------------------ Year (4 digits)
			//
			// This format does not seem to be recognized by
			// the standard DateTime parser.  A custom string
			// can be defined:
			//
			//   yyyyMMdd\THHmmss\Z

			if (DateTime.TryParseExact(
				  value,
				  @"yyyyMMdd\THHmmss\Z",
				  null,
				  DateTimeStyles.AssumeUniversal,
				  out parsed))
			{
				parsed = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
				return parsed;
			}

			return null;

		}

		#endregion

		#region [ ParseEncoding ]

		/// <summary>
		///     Parses an encoding name (such as "BASE64") and returns
		///     the corresponding <see cref="vCardEncoding"/> value.
		/// </summary>
		/// <param name="name">
		///     The name of an encoding from a standard vCard property.
		/// </param>
		/// <returns>
		///     The enumerated value of the encoding.
		/// </returns>
		public static vCardEncoding ParseEncoding(string name)
		{

			// If not specified, the default encoding (escaped) used
			// by the vCard file specification is assumed.

			if (string.IsNullOrEmpty(name))
				return vCardEncoding.Unknown;

			switch (name.ToUpperInvariant())
			{
				case "B":

					// Some vCard specification documents list the
					// encoding name "b" instead of "base64".

					return vCardEncoding.Base64;

				case "BASE64":
					return vCardEncoding.Base64;

				case "QUOTED-PRINTABLE":
					return vCardEncoding.QuotedPrintable;

				default:
					return vCardEncoding.Unknown;

			}

		}

		#endregion

		#region [ ParsePhoneType(string) ]

		/// <summary>
		///     Parses the name of a phone type and returns the
		///     corresponding <see cref="vCardPhoneTypes"/> value.
		/// </summary>
		/// <param name="name">
		///     The name of a phone type from a TEL vCard property.
		/// </param>
		/// <returns>
		///     The enumerated value of the phone type, or Default
		///     if the phone type could not be determined.
		/// </returns>
		public static vCardPhoneTypes ParsePhoneType(string name)
		{

			if (string.IsNullOrEmpty(name))
				return vCardPhoneTypes.Default;

			switch (name.Trim().ToUpperInvariant())
			{


				case "BBS":
					return vCardPhoneTypes.BBS;

				case "CAR":
					return vCardPhoneTypes.Car;

				case "CELL":
					return vCardPhoneTypes.Cellular;

				case "FAX":
					return vCardPhoneTypes.Fax;

				case "HOME":
					return vCardPhoneTypes.Home;

				case "ISDN":
					return vCardPhoneTypes.ISDN;

				case "MODEM":
					return vCardPhoneTypes.Modem;

				case "MSG":
					return vCardPhoneTypes.MessagingService;

				case "PAGER":
					return vCardPhoneTypes.Pager;

				case "PREF":
					return vCardPhoneTypes.Preferred;

				case "VIDEO":
					return vCardPhoneTypes.Video;

				case "VOICE":
					return vCardPhoneTypes.Voice;

				case "WORK":
					return vCardPhoneTypes.Work;
				case "IPHONE":
					return vCardPhoneTypes.IPhone;
				case "MAIN":
					return vCardPhoneTypes.Main;

				default:
					return vCardPhoneTypes.Default;
			}

		}

		#endregion

		#region [ ParsePhoneType(string[]) ]

		/// <summary>
		///     Decodes the bitmapped phone type given an array of
		///     phone type names.
		/// </summary>
		/// <param name="names">
		///     An array containing phone type names such as BBS or VOICE.
		/// </param>
		/// <returns>
		///     The phone type value that represents the combination
		///     of all names defined in the array.  Unknown names are
		///     ignored.
		/// </returns>
		public static vCardPhoneTypes ParsePhoneType(string[] names)
		{

			vCardPhoneTypes sum = vCardPhoneTypes.Default;

			foreach (string name in names)
			{
				sum |= ParsePhoneType(name);
			}

			return sum;

		}

		#endregion

		#region [ ParseDeliveryAddressType(string) ]

		/// <summary>
		///     Parses the type of postal address.
		/// </summary>
		/// <param name="value">
		///     The single value of a TYPE subproperty for the ADR property.
		/// </param>
		/// <returns>
		///     The <see cref="vCardDeliveryAddressTypes"/> that corresponds
		///     with the TYPE keyword, or vCardPostalAddressType.Default if
		///     the type could not be identified.
		/// </returns>
		public static vCardDeliveryAddressTypes ParseDeliveryAddressType(string value)
		{

			if (string.IsNullOrEmpty(value))
				return vCardDeliveryAddressTypes.Default;

			switch (value.ToUpperInvariant())
			{
				case "DOM":
					return vCardDeliveryAddressTypes.Domestic;

				case "HOME":
					return vCardDeliveryAddressTypes.Home;

				case "INTL":
					return vCardDeliveryAddressTypes.International;

				case "PARCEL":
					return vCardDeliveryAddressTypes.Parcel;

				case "POSTAL":
					return vCardDeliveryAddressTypes.Postal;

				case "WORK":
					return vCardDeliveryAddressTypes.Work;

				case "PREF":
					return vCardDeliveryAddressTypes.Preferred;

				default:
					return vCardDeliveryAddressTypes.Default;
			}

		}

		#endregion

		#region [ ParseDeliveryAddressType(string[]) ]

		/// <summary>
		///     Parses a string array containing one or more
		///     postal address types.
		/// </summary>
		/// <param name="typeNames">
		///     A string array containing zero or more keywords
		///     used with the TYPE subproperty of the ADR property.
		/// </param>
		/// <returns>
		///     A <see cref="vCardDeliveryAddressTypes"/> flags enumeration
		///     that corresponds with all known type names from the array.
		/// </returns>
		public static List<vCardDeliveryAddressTypes> ParseDeliveryAddressType(string[] typeNames)
		{
			List<vCardDeliveryAddressTypes> allTypes = new List<vCardDeliveryAddressTypes>();

			foreach (string typeName in typeNames)
			{
				allTypes.Add(ParseDeliveryAddressType(typeName));
			}

			return allTypes;

		}

		#endregion


		#region [ ReadInto(vCard, TextReader) ]

		/// <summary>
		///     Reads a vCard (VCF) file from an input stream.
		/// </summary>
		/// <param name="card">
		///     An initialized vCard.
		/// </param>
		/// <param name="reader">
		///     A text reader pointing to the beginning of
		///     a standard vCard file.
		/// </param>
		/// <returns>
		///     The vCard with values updated from the file.
		/// </returns>
		public override void ReadInto(vCard card, TextReader reader)
		{

			vCardProperty property;

			do
			{
				property = ReadProperty(reader);

				if (property != null)
				{

					if (
						(string.Compare("END", property.Name, StringComparison.OrdinalIgnoreCase) == 0) &&
						(string.Compare("VCARD", property.ToString(), StringComparison.OrdinalIgnoreCase) == 0))
					{

						// This is a special type of property that marks
						// the last property of the vCard. 

						break;
					}
					else
					{
						ReadInto(card, property);
					}
				}

			} while (property != null);

		}

		#endregion

		#region [ ReadInto(vCard, vCardProperty) ]

		/// <summary>
		///     Updates a vCard object based on the contents of a vCardProperty.
		/// </summary>
		/// <param name="card">
		///     An initialized vCard object.
		/// </param>
		/// <param name="property">
		///     An initialized vCardProperty object.
		/// </param>
		/// <remarks>
		///     <para>
		///         This method examines the contents of a property
		///         and attempts to update an existing vCard based on
		///         the property name and value.  This function must
		///         be updated when new vCard properties are implemented.
		///     </para>
		/// </remarks>
		public void ReadInto(vCard card, vCardProperty property)
		{

			if (card == null)
				throw new ArgumentNullException("card");

			if (property == null)
				throw new ArgumentNullException("property");

			if (string.IsNullOrEmpty(property.Name))
				return;

			string propNameToProcess = property.Name.ToUpperInvariant();

			var match = Regex.Match(propNameToProcess, @"^ITEM\d+\.");

			if (match != null && match.Success && match.Value != null)
			{
				propNameToProcess = propNameToProcess.Replace(match.Value, string.Empty);
			}



			switch (propNameToProcess)
			{

				case "ADR":
					ReadInto_ADR(card, property);
					break;

				case "BDAY":
					ReadInto_BDAY(card, property);
					break;

				case "CATEGORIES":
					ReadInto_CATEGORIES(card, property);
					break;

				case "CLASS":
					ReadInto_CLASS(card, property);
					break;

				case "EMAIL":
					ReadInto_EMAIL(card, property);
					break;

				case "FN":
					ReadInto_FN(card, property);
					break;

				case "GEO":
					ReadInto_GEO(card, property);
					break;

				case "IMPP":
					ReadInto_IMPP(card, property);
					break;
				case "KEY":
					ReadInto_KEY(card, property);
					break;

				case "LABEL":
					ReadInto_LABEL(card, property);
					break;

				case "MAILER":
					ReadInto_MAILER(card, property);
					break;

				case "N":
					ReadInto_N(card, property);
					break;

				case "NAME":
					ReadInto_NAME(card, property);
					break;

				case "NICKNAME":
					ReadInto_NICKNAME(card, property);
					break;

				case "NOTE":
					ReadInto_NOTE(card, property);
					break;

				case "ORG":
					ReadInto_ORG(card, property);
					break;

				case "PHOTO":
					ReadInto_PHOTO(card, property);
					break;

				case "PRODID":
					ReadInto_PRODID(card, property);
					break;

				case "REV":
					ReadInto_REV(card, property);
					break;

				case "ROLE":
					ReadInto_ROLE(card, property);
					break;

				case "SOURCE":
					ReadInto_SOURCE(card, property);
					break;

				case "TEL":
					ReadInto_TEL(card, property);
					break;

				case "TITLE":
					ReadInto_TITLE(card, property);
					break;

				case "TZ":
					ReadInto_TZ(card, property);
					break;

				case "UID":
					ReadInto_UID(card, property);
					break;

				case "URL":
					ReadInto_URL(card, property);
					break;

				case "X-SOCIALPROFILE":
					ReadInto_XSocialProfile(card, property);
					break;

				case "X-WAB-GENDER":
					ReadInto_X_WAB_GENDER(card, property);
					break;

				default:

					// The property name is not recognized and
					// will be ignored.

					break;

			}

		}

		#endregion

		// The following functions (ReadInfo_xxx) implement the logic
		// for each recognized vCard property.  A separate function
		// for each property is implemented for easier organization.
		//
		// Each function is a private function.  It is not necessary
		// for a function to double-check the name of a property, or
		// check for null parameters.

		#region [ ReadInto_ADR() ]

		/// <summary>
		///     Reads an ADR property.
		/// </summary>
		private void ReadInto_ADR(vCard card, vCardProperty property)
		{

			// The ADR property defines a delivery address, such
			// as a home postal address.  The property contains
			// the following components separated by semicolons:
			//
			//   0. Post office box
			//   1. Extended address
			//   2. Street
			//   3. Locality (e.g. city)
			//   4. Region (e.g. state or province)
			//   5. Postal code
			//   6. Country name
			//
			// This version of the reader ignores any ADR properties
			// with a lesser number of components.  If more than 7
			// components exist, then the lower seven components are
			// assumed to still match the specification (e.g. the
			// additional components may be from a future specification).

			string[] addressParts =
				property.Value.ToString().Split(new char[] { ';' });

			vCardDeliveryAddress deliveryAddress = new vCardDeliveryAddress();

			if (addressParts.Length >= 7)
				deliveryAddress.Country = addressParts[6].Trim();

			if (addressParts.Length >= 6)
				deliveryAddress.PostalCode = addressParts[5].Trim();

			if (addressParts.Length >= 5)
				deliveryAddress.Region = addressParts[4].Trim();

			if (addressParts.Length >= 4)
				deliveryAddress.City = addressParts[3].Trim();

			if (addressParts.Length >= 3)
				deliveryAddress.Street = addressParts[2].Trim();

			if (
				(string.IsNullOrEmpty(deliveryAddress.City)) &&
				(string.IsNullOrEmpty(deliveryAddress.Country)) &&
				(string.IsNullOrEmpty(deliveryAddress.PostalCode)) &&
				(string.IsNullOrEmpty(deliveryAddress.Region)) &&
				(string.IsNullOrEmpty(deliveryAddress.Street)))
			{

				// No address appears to be defined.
				// Ignore.

				return;

			}

			// Handle the old 2.1 format in which the ADR type names (e.g.
			// DOM, HOME, etc) were written directly as subproperties.
			// For example, "ADR;HOME;POSTAL:...".

			deliveryAddress.AddressType =
				ParseDeliveryAddressType(property.Subproperties.GetNames(DeliveryAddressTypeNames));

			// Handle the new 3.0 format in which the delivery address
			// type is a comma-delimited list, e.g. "ADR;TYPE=HOME,POSTAL:".
			// It is possible for the TYPE subproperty to be listed multiple
			// times (this is allowed by the RFC, although irritating that
			// the authors allowed it).
			List<vCardDeliveryAddressTypes> addressTypes = new List<vCardDeliveryAddressTypes>();
			foreach (vCardSubproperty sub in property.Subproperties)
			{

				// If this subproperty is a TYPE subproperty and
				// has a non-null value, then parse it.

				if (
					(!string.IsNullOrEmpty(sub.Value)) &&
					(string.Compare("TYPE", sub.Name, StringComparison.OrdinalIgnoreCase) == 0))
				{

					addressTypes.AddRange(ParseDeliveryAddressType(sub.Value.Split(new char[] { ',' })));

				}

			}

			if (addressTypes.Count != 0)
			{
				deliveryAddress.AddressType = addressTypes;
			}

			card.DeliveryAddresses.Add(deliveryAddress);

		}

		#endregion

		#region [ ReadInto_BDAY ]

		/// <summary>
		///     Reads the BDAY property.
		/// </summary>
		private void ReadInto_BDAY(vCard card, vCardProperty property)
		{

			DateTime bday;
			if (DateTime.TryParse(property.ToString(), out bday))
			{
				card.BirthDate = bday;
			}
			else
			{

				// Microsoft Outlook writes the birthdate in YYYYMMDD, e.g. 20091015
				// for October 15, 2009.

				if (DateTime.TryParseExact(
					property.ToString(),
					"yyyyMMdd",
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out bday))
				{
					card.BirthDate = bday;
				}
				else
				{
					card.BirthDate = null;
				}
			}

		}

		#endregion

		#region [ ReadInto_CATEGORIES ]

		/// <summary>
		///     Reads the CATEGORIES property.
		/// </summary>
		private void ReadInto_CATEGORIES(vCard card, vCardProperty property)
		{

			// The CATEGORIES value is expected to be a comma-delimited list.

			string[] cats = property.ToString().Split(new char[] { ',' });

			// Add each non-blank line to the categories collection.

			foreach (string cat in cats)
			{
				if (cat.Length > 0)
					card.Categories.Add(cat);
			}

		}

		#endregion

		#region [ ReadInto_CLASS ]

		/// <summary>
		///     Reads the CLASS property.
		/// </summary>
		private void ReadInto_CLASS(vCard card, vCardProperty property)
		{

			if (property.Value == null)
				return;

			switch (property.ToString().ToUpperInvariant())
			{
				case "PUBLIC":
					card.AccessClassification = vCardAccessClassification.Public;
					break;

				case "PRIVATE":
					card.AccessClassification = vCardAccessClassification.Private;
					break;

				case "CONFIDENTIAL":
					card.AccessClassification = vCardAccessClassification.Confidential;
					break;
			}

		}

		#endregion

		#region [ ReadInto_EMAIL ]

		/// <summary>
		///     Reads an EMAIL property.
		/// </summary>
		private void ReadInto_EMAIL(vCard card, vCardProperty property)
		{

			vCardEmailAddress email = new vCardEmailAddress();

			// The email address is stored as the value of the property.
			// The format of the address depends on the type of email
			// address.  The current version of the library does not
			// perform any validation.

			email.Address = property.Value.ToString();

			// Loop through each subproperty and look for flags
			// that indicate the type of email address.

			foreach (vCardSubproperty subproperty in property.Subproperties)
			{

				switch (subproperty.Name.ToUpperInvariant())
				{

					case "PREF":

						// The PREF subproperty indicates the email
						// address is the preferred email address to
						// use when contacting the person.

						email.IsPreferred = true;
						break;

					case "TYPE":

						// The TYPE subproperty is new in vCard 3.0.
						// It identifies the type and can also indicate
						// the PREF attribute.

						string[] typeValues =
							subproperty.Value.Split(new char[] { ',' });

						foreach (string typeValue in typeValues)
						{
							if (string.Compare("PREF", typeValue, StringComparison.OrdinalIgnoreCase) == 0)
							{
								email.IsPreferred = true;
							}
							else
							{
								vCardEmailAddressType? typeType = DecodeEmailAddressType(typeValue);

								if (typeType.HasValue)
								{
									email.EmailType = typeType.Value;
								}
								else
								{

									ItemType? itemType = DecodeItemType(typeValue);

									if (itemType.HasValue)
									{
										email.ItemType = itemType.Value;
									}

								}
							}

						}
						break;

					default:

						// All other subproperties are probably vCard 2.1
						// subproperties.  This was before the email type
						// was supposed to be specified with TYPE=VALUE.

						vCardEmailAddressType? emailType =
							DecodeEmailAddressType(subproperty.Name);

						if (emailType.HasValue)
							email.EmailType = emailType.Value;

						break;

				}

			}

			card.EmailAddresses.Add(email);

		}

		#endregion

		#region [ ReadInto_FN ]

		/// <summary>
		///     Reads the FN property.
		/// </summary>
		private void ReadInto_FN(vCard card, vCardProperty property)
		{

			// The FN property defines the formatted display name
			// of the person.  This is used for presentation.

			card.FormattedName = property.Value.ToString();

		}

		#endregion

		#region [ ReadInfo_GEO ]

		/// <summary>
		///     Reads the GEO property.
		/// </summary>
		private void ReadInto_GEO(vCard card, vCardProperty property)
		{

			// The GEO property specifies latitude and longitude
			// of the entity associated with the vCard.

			string[] coordinates =
				property.Value.ToString().Split(new char[] { ';' });

			if (coordinates.Length == 2)
			{
				float geoLatitude;
				float geoLongitude;

				if (
					float.TryParse(coordinates[0], out geoLatitude) &&
					float.TryParse(coordinates[1], out geoLongitude))
				{
					card.Latitude = geoLatitude;
					card.Longitude = geoLongitude;
				}

			}

		}

		#endregion

        private vCardIMPP ParseFullIMHandleString(string fullIMHandle)
        {
            var im = new vCardIMPP();
            string[] parsedTypeValues = fullIMHandle.Split(new char[] { ':' });
            if (parsedTypeValues.Length > 1)
            {
                var typeValueToCheck = parsedTypeValues[0];

                if (string.IsNullOrEmpty(typeValueToCheck))
                {
                    typeValueToCheck = parsedTypeValues[1];
                }

                if (fullIMHandle.StartsWith(":"))
                {
                    fullIMHandle = fullIMHandle.Substring(1);
                }
                string directHandle = parsedTypeValues[parsedTypeValues.Length - 1];

                //need to switch to this => GoogleTalk:xmpp:gtalkname
                im.ServiceType = IMTypeUtils.GetIMServiceType(typeValueToCheck).Value;
                im.Handle = directHandle;
                
            }

            return im;
        }

		private void ReadInto_IMPP(vCard card, vCardProperty property)
		{

			vCardIMPP im = new vCardIMPP();

			// The full telephone number is stored as the 
			// value of the property.  Currently no formatted
			// rules are applied since the vCard specification
			// is somewhat confusing on this matter.

			im.Handle = property.ToString();
			if (string.IsNullOrEmpty(im.Handle))
				return;

            if (property.Subproperties.Count == 0)
            {
                im = ParseFullIMHandleString(im.Handle);
            }

			foreach (vCardSubproperty subproperty in property.Subproperties)
			{

				switch (subproperty.Name.ToUpperInvariant())
				{

					case "PREF":

						// The PREF subproperty indicates the email
						// address is the preferred email address to
						// use when contacting the person.

						im.IsPreferred = true;
						break;

					case "TYPE":
					case "X-SERVICE-TYPE":
						// The TYPE subproperty is new in vCard 3.0.
						// It identifies the type and can also indicate
						// the PREF attribute.

						if (subproperty.Value != null)
						{
							string[] typeValues = subproperty.Value.Split(new char[] { ',' });

							foreach (string typeValue in typeValues)
							{
								string typeValueToCheck = typeValue;

								if (string.Compare("PREF", typeValueToCheck, StringComparison.OrdinalIgnoreCase) == 0)
								{
									im.IsPreferred = true;
								}
								else
								{
									//parsing from em-Client version of supplying IM's
									//:google:aqibtalib@gtalk.com
									if (im.Handle != null && typeValueToCheck == "OTHER")
									{
                                        im = ParseFullIMHandleString(im.Handle);

                                        break;
									}

									IMServiceType? imServiceType = IMTypeUtils.GetIMServiceType(typeValueToCheck);

									if (imServiceType.HasValue)
									{
										im.ServiceType = imServiceType.Value;

										//fix handle
										im.Handle = IMTypeUtils.StripHandlePrefix(im.ServiceType, im.Handle);

									}
									else
									{
										ItemType? itemType = DecodeItemType(typeValueToCheck);

										if (itemType.HasValue)
										{
											im.ItemType = itemType.Value;
										}
									}
								}
							}
						}

						break;

				}

			}

			card.IMs.Add(im);

		}



		#region [ ReadInto_KEY ]

		/// <summary>
		///     Reads the KEY property.
		/// </summary>
		private void ReadInto_KEY(vCard card, vCardProperty property)
		{

			// The KEY property defines a security certificate
			// that has been attached to the vCard.  Key values
			// are usually encoded in BASE64 because they
			// often consist of binary data.

			vCardCertificate certificate = new vCardCertificate();
			certificate.Data = (byte[])property.Value;

			// TODO: Support other key types.

			if (property.Subproperties.Contains("X509"))
				certificate.KeyType = "X509";

			card.Certificates.Add(certificate);

		}

		#endregion

		#region [ ReadInto_LABEL ]

		/// <summary>
		///     Reads the LABEL property.
		/// </summary>
		private void ReadInto_LABEL(vCard card, vCardProperty property)
		{

			vCardDeliveryLabel label = new vCardDeliveryLabel();

			label.Text = property.Value.ToString();

			// Handle the old 2.1 format in which the ADR type names (e.g.
			// DOM, HOME, etc) were written directly as subproperties.
			// For example, "LABEL;HOME;POSTAL:...".

			label.AddressType =
				ParseDeliveryAddressType(property.Subproperties.GetNames(DeliveryAddressTypeNames));

			// Handle the new 3.0 format in which the delivery address
			// type is a comma-delimited list, e.g. "ADR;TYPE=HOME,POSTAL:".
			// It is possible for the TYPE subproperty to be listed multiple
			// times (this is allowed by the RFC, although irritating that
			// the authors allowed it).

			foreach (vCardSubproperty sub in property.Subproperties)
			{

				// If this subproperty is a TYPE subproperty and
				// has a non-null value, then parse it.

				if (
					(!string.IsNullOrEmpty(sub.Value)) &&
					(string.Compare("TYPE", sub.Name, StringComparison.OrdinalIgnoreCase) == 0))
				{

					label.AddressType =
						ParseDeliveryAddressType(sub.Value.Split(new char[] { ',' }));

				}

			}

			card.DeliveryLabels.Add(label);


		}

		#endregion

		#region [ ReadInto_MAILER ]

		/// <summary>
		///     Reads the MAILER property.
		/// </summary>
		private void ReadInto_MAILER(vCard card, vCardProperty property)
		{

			// The MAILER property identifies the mail software
			// used by the person.  This can be examined by a 
			// program to detect software-specific conventions.
			// See section 2.4.3 of the vCard 2.1 spec.  This
			// property is not common.

			card.Mailer = property.Value.ToString();

		}

		#endregion

		#region [ ReadInto_N ]

		/// <summary>
		///     Reads the N property.
		/// </summary>
		private void ReadInto_N(vCard card, vCardProperty property)
		{

			// The N property defines the name of the person. The
			// propery value has several components, such as the
			// given name, family name, and suffix.  This is a 
			// core field found in almost all vCards.
			//
			// Each component is supposed to be separated with
			// a semicolon.  However, some vCard writers do not
			// write out training semicolons.  For example, the
			// last two components are the prefix (e.g. Mr.)
			// and suffix (e.g. Jr) of the name.  The semicolons
			// will be missing in some vCards if these components
			// are blank.

			string[] names = property.ToString().Split(';');

			// The first value is the family (last) name.
			card.FamilyName = names[0];
			if (names.Length == 1)
				return;

			// The next value is the given (first) name.
			card.GivenName = names[1];
			if (names.Length == 2)
				return;

			// The next value contains the middle name.
			card.AdditionalNames = names[2];
			if (names.Length == 3)
				return;

			// The next value contains the prefix, e.g. Mr.
			card.NamePrefix = names[3];
			if (names.Length == 4)
				return;

			// The last value contains the suffix, e.g. Jr.
			card.NameSuffix = names[4];

		}

		#endregion

		#region [ ReadInto_NAME ]

		/// <summary>
		///     Reads the NAME property.
		/// </summary>
		private void ReadInto_NAME(vCard card, vCardProperty property)
		{

			// The NAME property is used to define the displayable
			// name of the vCard.  Because it is intended for display
			// purposes, any whitespace at the beginning or end of
			// the name is trimmed.

			card.DisplayName = property.ToString().Trim();
		}

		#endregion

		#region [ ReadInto_NICKNAME ]

		/// <summary>
		///     Reads the NICKNAME property.
		/// </summary>
		private void ReadInto_NICKNAME(vCard card, vCardProperty property)
		{

			if (property.Value == null)
				return;

			// The nicknames are comma-separated values.

			string[] nicknames =
				property.Value.ToString().Split(new char[] { ',' });

			foreach (string nickname in nicknames)
			{

				string trimmedNickname = nickname.Trim();
				if (trimmedNickname.Length > 0)
				{
					card.Nicknames.Add(trimmedNickname);
				}

			}

		}

		#endregion

		#region [ ReadInto_NOTE ]

		/// <summary>
		///     Reads the NOTE property.
		/// </summary>
		private void ReadInto_NOTE(vCard card, vCardProperty property)
		{

			if (property.Value != null)
			{

				vCardNote note = new vCardNote();

				note.Language = property.Subproperties.GetValue("language");
				note.Text = property.Value.ToString();

				if (!string.IsNullOrEmpty(note.Text))
				{
					card.Notes.Add(note);
				}

			}

		}

		#endregion

		#region [ ReadInto_ORG ]

		/// <summary>
		///     Reads the ORG property.
		/// </summary>
		private void ReadInto_ORG(vCard card, vCardProperty property)
		{

			// The ORG property contains the name of the company
			// or organization of the person.

			card.Organization = property.Value.ToString();

			if (card.Organization != null && card.Organization.EndsWith(";"))
			{
				card.Organization = card.Organization.TrimEnd(Convert.ToChar(";"));
			}

		}

		#endregion

		#region [ ReadInto_PHOTO ]

		/// <summary>
		///     Reads the PHOTO property.
		/// </summary>
		private void ReadInto_PHOTO(vCard card, vCardProperty property)
		{

			// The PHOTO property contains an embedded (encoded) image
			// or a link to an image.  A URL (linked) image is supposed
			// to be indicated with the VALUE=URI subproperty.

			string valueType = property.Subproperties.GetValue("VALUE");

			//URI is the standard, but I've seen examples online of URL
			if ((string.Compare(valueType, "URI", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(valueType, "URL", StringComparison.OrdinalIgnoreCase) == 0))
			{

				// This image has been defined as a URI/URL link, 
				// rather than being encoded directly in the vCard.

				card.Photos.Add(
					new vCardPhoto(new Uri(property.ToString())));


			}
			else
			{
				if (property.Value.GetType() == typeof(string))
				{
					card.Photos.Add(new vCardPhoto((string)property.Value, true));
				}
				else
				{
					card.Photos.Add(new vCardPhoto((byte[])property.Value));
				}

			}
		}

		#endregion

		#region [ ReadInto_PRODID ]

		/// <summary>
		///     Reads the PRODID property.
		/// </summary>
		private void ReadInto_PRODID(vCard card, vCardProperty property)
		{

			// The PRODID property contains the name of the
			// software that generated the vCard.  This is not
			// a common property.  Also note: this library
			// does not automatically generate a PRODID when
			// creating a vCard file.  The developer can set
			// the PRODID (via the ProductId parameter) to
			// anything desired.

			card.ProductId = property.ToString();

		}

		#endregion

		#region [ ReadInto_REV ]

		/// <summary>
		///     Reads the REV property.
		/// </summary>
		private void ReadInto_REV(vCard card, vCardProperty property)
		{

			// The REV property indicates the last revision date
			// of the vCard.  Note that Outlook and perhaps other
			// clients generate the revision date in a format not
			// recognized directly by the .NET DateTime parser.
			// A custom format is used; see ParseDate for details.

			card.RevisionDate = ParseDate(property.Value.ToString());

		}

		#endregion

		#region [ ReadInto_ROLE ]

		/// <summary>
		///     Reads the ROLE property.
		/// </summary>
		private void ReadInto_ROLE(vCard card, vCardProperty property)
		{

			// The ROLE property describes the role of the
			// person at his/her organization (e.g. Programmer
			// or Executive, etc).

			card.Role = property.Value.ToString();

		}

		#endregion

		#region [ ReadInto_SOURCE ]

		/// <summary>
		///     Reads the SOURCE property.
		/// </summary>
		private void ReadInto_SOURCE(vCard card, vCardProperty property)
		{

			// The SOURCE property identifies the source of
			// directory information (e.g. an LDAP server).  This
			// is not widely supported.  See RFC 2425, sec. 6.1.

			vCardSource source = new vCardSource();
			source.Context = property.Subproperties.GetValue("CONTEXT");
			source.Uri = new Uri(property.Value.ToString());
			card.Sources.Add(source);

		}

		#endregion

		#region [ ReadInto_TEL ]

		/// <summary>
		///     Reads the TEL property.
		/// </summary>
		private void ReadInto_TEL(vCard card, vCardProperty property)
		{

			vCardPhone phone = new vCardPhone();

			// The full telephone number is stored as the 
			// value of the property.  Currently no formatted
			// rules are applied since the vCard specification
			// is somewhat confusing on this matter.

			phone.FullNumber = property.ToString();
			if (string.IsNullOrEmpty(phone.FullNumber))
				return;

			foreach (vCardSubproperty sub in property.Subproperties)
			{

				// If this subproperty is a TYPE subproperty
				// and it has a value, then it is expected
				// to contain a comma-delimited list of phone types.

				if (
					(string.Compare(sub.Name, "TYPE", StringComparison.OrdinalIgnoreCase) == 0) &&
					(!string.IsNullOrEmpty(sub.Value)))
				{
					// This is a vCard 3.0 subproperty.  It defines the
					// the list of phone types in a comma-delimited list.
					// Note that the vCard specification allows for
					// multiple TYPE subproperties (why ?!).

					phone.PhoneType |=
						ParsePhoneType(sub.Value.Split(new char[] { ',' }));

				}
				else
				{

					// The other subproperties in a TEL property
					// define the phone type.  The only exception
					// are meta fields like ENCODING, CHARSET, etc,
					// but these are probably rare with TEL.

					phone.PhoneType |= ParsePhoneType(sub.Name);

				}

			}

			card.Phones.Add(phone);

		}

		#endregion

		#region [ ReadInto_TITLE ]

		/// <summary>
		///     Reads the TITLE property.
		/// </summary>
		private void ReadInto_TITLE(vCard card, vCardProperty property)
		{

			// The TITLE property defines the job title of the
			// person.  This should not be confused by the name
			// prefix (e.g. "Mr"), which is called "Title" in
			// some vCard-compatible software like Outlook.

			card.Title = property.ToString();

		}

		#endregion

		#region [ ReadInto_TZ ]

		/// <summary>
		///     Reads a TZ property.
		/// </summary>
		private void ReadInto_TZ(vCard card, vCardProperty property)
		{
			card.TimeZone = property.ToString();
		}

		#endregion

		#region [ ReadInto_UID ]

		/// <summary>
		///     Reads the UID property.
		/// </summary>
		private void ReadInto_UID(vCard card, vCardProperty property)
		{
			card.UniqueId = property.ToString();
		}

		#endregion

		#region [ ReadInto_URL ]

		/// <summary>
		///     Reads the URL property.
		/// </summary>
		private void ReadInto_URL(vCard card, vCardProperty property)
		{

			vCardWebsite webSite = new vCardWebsite();

			webSite.Url = property.ToString();

			if (property.Subproperties.Contains("HOME"))
				webSite.IsPersonalSite = true;

			if (property.Subproperties.Contains("WORK"))
				webSite.IsWorkSite = true;

			card.Websites.Add(webSite);

		}

		#endregion


		private void ReadInto_XSocialProfile(vCard card, vCardProperty property)
		{

			vCardSocialProfile sp = new vCardSocialProfile();


			sp.ProfileUrl = property.ToString();
			if (string.IsNullOrEmpty(sp.ProfileUrl))
				return;

			foreach (vCardSubproperty subproperty in property.Subproperties)
			{

				switch (subproperty.Name.ToUpperInvariant())
				{

					case "X-USER":

						sp.Username = subproperty.Value;

						break;

					case "TYPE":


						string[] typeValues =
							subproperty.Value.Split(new char[] { ',' });

						foreach (string typeValue in typeValues)
						{

							SocialProfileServiceType? profileType = SocialProfileTypeUtils.GetSocialProfileServiceType(typeValue);


							if (profileType.HasValue)
							{
								sp.ServiceType = profileType.Value;


							}




						}
						break;

				}

			}

			card.SocialProfiles.Add(sp);

		}

		#region [ ReadInto_X_WAB_GENDER ]

		/// <summary>
		///     Reads the X-WAB-GENDER property.
		/// </summary>
		private void ReadInto_X_WAB_GENDER(vCard card, vCardProperty property)
		{

			// The X-WAB-GENDER property is a custom property generated by
			// Microsoft Outlook 2003.  It contains the value 1 for females
			// or 2 for males.  It is not known if other PIM clients 
			// recognize this value.

			int genderId;

			if (int.TryParse(property.ToString(), out genderId))
			{
				switch (genderId)
				{
					case 1:
						card.Gender = vCardGender.Female;
						break;

					case 2:
						card.Gender = vCardGender.Male;
						break;
				}
			}

		}

		#endregion

		#region [ ReadProperty(string) ]

		/// <summary>
		///     Reads a property from a string.
		/// </summary>
		public vCardProperty ReadProperty(string text)
		{

			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException("text");

			using (StringReader reader = new StringReader(text))
			{
				return ReadProperty(reader);
			}

		}

		#endregion

		#region [ ReadProperty(TextReader) ]

		/// <summary>
		///     Reads a property from a text reader.
		/// </summary>
		public vCardProperty ReadProperty(TextReader reader)
		{

			if (reader == null)
				throw new ArgumentNullException("reader");

			do
			{

				// Read the first line of the next property
				// from the input stream.  If a null string
				// is returned, then the end of the input
				// stream has been reached.

				string firstLine = reader.ReadLine();
				if (firstLine == null)
					return null;

				// See if this line is a blank line.  It is
				// blank if (a) it has no characters, or (b)
				// it consists of whitespace characters only.

				firstLine = firstLine.Trim();
				if (firstLine.Length == 0)
				{
					Warnings.Add(Thought.vCards.WarningMessages.BlankLine);
					continue;
				}

				// Get the index of the colon (:) in this
				// property line.  All vCard properties are
				// written in NAME:VALUE format.

				int colonIndex = firstLine.IndexOf(':');
				if (colonIndex == -1)
				{
					Warnings.Add(Thought.vCards.WarningMessages.ColonMissing);
					continue;
				}

				// Get the name portion of the property.  This
				// portion contains the property name as well
				// as any subproperties.

				string namePart = firstLine.Substring(0, colonIndex).Trim();
				if (string.IsNullOrEmpty(namePart))
				{
					Warnings.Add(Thought.vCards.WarningMessages.EmptyName);
					continue;
				}

				// Split apart the name portion of the property.
				// A property can have subproperties, separated
				// by semicolons.

				string[] nameParts = namePart.Split(';');
				for (int i = 0; i < nameParts.Length; i++)
					nameParts[i] = nameParts[i].Trim();

				// The name of the property is supposed to
				// be first on the line.  An empty name is not
				// legal syntax.

				if (nameParts[0].Length == 0)
				{
					Warnings.Add(Thought.vCards.WarningMessages.EmptyName);
					continue;
				}

				// At this point there is sufficient text
				// to define a vCard property.  The only
				// true minimum requirement is a name.

				vCardProperty property = new vCardProperty();
				property.Name = nameParts[0];

				// Next, store any subproperties.  Subproperties
				// are defined like "NAME;SUBNAME=VALUE:VALUE".  Note
				// that subproperties do not necessarily have to have
				// a subvalue.

				for (int index = 1; index < nameParts.Length; index++)
				{

					// Split the subproperty into its name and 
					// value components.  If multiple equal signs
					// happen to exist, they are interpreted as
					// part of the value.  This may change in a 
					// future version of the parser.

					string[] subNameValue =
						nameParts[index].Split(new char[] { '=' }, 2);

					if (subNameValue.Length == 1)
					{

						// The Split function above returned a single
						// array element.  This means no equal (=) sign
						// was present.  The subproperty consists of
						// a name only.

						property.Subproperties.Add(
							nameParts[index].Trim());
					}
					else
					{
						property.Subproperties.Add(
							subNameValue[0].Trim(),
							subNameValue[1].Trim());
					}

				}

				// The subproperties have been defined.  The next
				// step is to try to identify the encoding of the
				// value.  The encoding is supposed to be specified
				// with a subproperty called ENCODING.  However, older
				// versions of the format just wrote the plain
				// encoding value, e.g. "NAME;BASE64:VALUE" instead
				// of the normalized "NAME;ENCODING=BASE64:VALUE" form.

				string encodingName =
					property.Subproperties.GetValue("ENCODING",
						new string[] { "B", "BASE64", "QUOTED-PRINTABLE" });

				var hasCharset = property.Subproperties.Contains("CHARSET");
				var charsetEncoding = Encoding.Default;
				if (hasCharset)
				{
					var charsetEncodingName = property.Subproperties.GetValue("CHARSET");
					charsetEncoding = GetCharsetEncoding(charsetEncodingName);
				}

				// Convert the encoding name into its corresponding
				// vCardEncoding enumeration value.

				vCardEncoding encoding =
					ParseEncoding(encodingName);

				// At this point, the first line of the property has been
				// loaded and the suggested value encoding has been
				// determined.  Get the raw value as encoded in the file.

				string rawValue = firstLine.Substring(colonIndex + 1);

				// The vCard specification allows long values
				// to be folded across multiple lines.  An example
				// is a security key encoded in MIME format.
				// When folded, each subsequent line begins with
				// a space or tab character instead of the next property.
				//
				// See: RFC 2425, Section 5.8.1

				do
				{
					int peekChar = reader.Peek();

					if ((peekChar == 32) || (peekChar == 9))
					{
						string foldedLine = reader.ReadLine();
						rawValue += foldedLine.Substring(1);
					}
					else
					{
						break;
					}

				} while (true);

				if (encoding == vCardEncoding.QuotedPrintable && rawValue.Length > 0)
				{
					while (rawValue[rawValue.Length - 1] == '=')
					{
						rawValue += "\r\n" + reader.ReadLine();
					}
				}

				// The full value has finally been loaded from the
				// input stream.  The next step is to decode it.

				switch (encoding)
				{
					case vCardEncoding.Base64:
						property.Value = DecodeBase64(rawValue);
						break;

					case vCardEncoding.Escaped:
						property.Value = DecodeEscaped(rawValue);
						break;

					case vCardEncoding.QuotedPrintable:
						property.Value = DecodeQuotedPrintable(rawValue, charsetEncoding);
						break;

					default:
						property.Value = DecodeEscaped(rawValue);
						break;
				}

				return property;

			} while (true);

		}


		private Encoding GetCharsetEncoding(string encodingName)
		{
			switch (encodingName)
			{
				case "UTF-8":
					return Encoding.UTF8;
				case "ASCII":
					return Encoding.ASCII;
				default:
					return Encoding.GetEncoding(encodingName);
			}
		}
		#endregion

	}

}