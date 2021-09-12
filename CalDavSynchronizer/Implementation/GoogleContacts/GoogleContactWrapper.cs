using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Contacts;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
    public class GoogleContactWrapper
    {
        public GoogleContactWrapper(Contact contact)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            Contact = contact;
        }

        public byte[] PhotoOrNull { get; set; }
        public Contact Contact { get; }
        public List<string> Groups { get; } = new List<string>();
    }
}