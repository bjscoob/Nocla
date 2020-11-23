using System;
using System.Collections.Generic;
using System.Text;

namespace Nocla.Models
{
    public class Contact
    {
        public int contactId;
        public string username;
        public string firstname;
        public string lastname;
        public string imageStr;
        public bool isContactGroup;
        public List<Contact> innerContacts;
        public string toString() {
            return string.Format("{0} {1}({2})",firstname,lastname, username.ToUpper());
        }

    }
}
