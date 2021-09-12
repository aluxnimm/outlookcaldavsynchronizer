using System;
using System.IO;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;

namespace CalDavSynchronizer.DDayICalWorkaround
{
    public class CustomAttendeeSerializer : StringSerializer
    {
        public override Type TargetType
        {
            get { return typeof(Attendee); }
        }

        public override string SerializeToString(object obj)
        {
            IAttendee a = obj as IAttendee;
            if (a != null && a.Value != null)
                return Encode(a, a.Value.OriginalString);
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            IAttendee a = null;
            try
            {
                a = CreateAndAssociate() as IAttendee;
                if (a != null)
                {
                    string uriString = Unescape(Decode(a, value));

                    // Prepend "mailto:" only if uriString doesn't start with mailto: or urn: 
                    if (!(uriString.StartsWith("mailto:", StringComparison.InvariantCultureIgnoreCase) ||
                          uriString.StartsWith("urn:", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        uriString = "mailto:" + uriString;
                    }

                    a.Value = new Uri(uriString);
                }
            }
            catch
            {
            }

            return a;
        }
    }
}