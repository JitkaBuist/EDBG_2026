using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Security.Principal;  // here is the security namespace you need


namespace Energie.DataAccess
{
    public class User
    {

        public String UserName
        {
           // set { emptypossible = value; }
            get { return WindowsIdentity.GetCurrent().Name; }
        }

        public Int16 UserID
        {
            // set { emptypossible = value; }
            get { return 1; }
        }

    }
}
