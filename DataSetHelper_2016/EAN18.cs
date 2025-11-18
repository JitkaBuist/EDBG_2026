using System;
using System.Collections.Generic;
using System.Text;

namespace Energie.DataTableHelper
{
    /// <summary>
    /// Summary description for EAN18.
    /// </summary>
    [Serializable]
    public struct EAN18 : IComparable, IFormattable
    {
        #region Fields

        private const long MinValue = 100000000000000000;
        private const long MaxValue = 999999999999999999;

        public static readonly EAN18 Null = new EAN18(MinValue);

        private long value;

        #endregion

        #region Constructors


        public EAN18(long value2)
        {

            if ((value2 < MinValue || value2 > MaxValue))
            {
                throw new OverflowException("Value not within range");
            }
            if (value2 != MinValue)
            {
                int counter1 = (value2.ToString().Length - 1); // -1 voor 17 parsen als 18 invoer en -1 voor start 0??
                int counter2 = 1;
                int checksum = 0;
                while (counter1 > 0)
                {
                    if (counter2 % 2 == 1)
                    {
                        //odd
                        checksum = checksum + 3 * int.Parse(value2.ToString().Substring((counter1 - 1), 1));
                    }
                    else
                    {
                        //even
                        checksum = checksum + int.Parse(value2.ToString().Substring((counter1 - 1), 1));
                    }
                    counter1 = counter1 - 1;
                    counter2 = counter2 + 1;
                }
                int delta = 0;
                delta = (10 - checksum % 10) - int.Parse(value2.ToString().Substring((value2.ToString().Length - 1), 1));
                if (delta == 0 || delta == 10)
                {
                    //all ok                
                }
                else
                {
                    throw new OverflowException("Mod 10 Check Failed");
                }
            }
            this.value = value2;

        }

        #endregion

        #region Properties

        public long Value
        {
            get { return this.value; }
        }

        public bool IsNull
        {
            get { return (this == EAN18.Null); }
        }

        public bool IsNotNull
        {
            get { return (!this.IsNull); }
        }

        #endregion

        #region Methods

        public static EAN18 Parse(string s)
        {
            return new EAN18(long.Parse(s));
        }

        public override string ToString()
        {
            if (this.IsNull)
            {
                return String.Empty;
            }
            return this.value.ToString("000000000000000000");
        }

        public override bool Equals(object obj)
        {
            if (obj is EAN18)
            {
                EAN18 ean18Obj = (EAN18)obj;
                return this.value.Equals(ean18Obj.value);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        #endregion

        #region Operators

        public static bool operator ==(EAN18 e1, EAN18 e2)
        {
            return e1.value == e2.value;
        }

        public static bool operator !=(EAN18 e1, EAN18 e2)
        {
            return e1.value != e2.value;
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (!(obj is EAN18))
            {
                throw new ArgumentException("Argument must be of type " + this.GetType());
            }

            EAN18 ean18Obj = (EAN18)obj;

            return this.value.CompareTo(ean18Obj.value);
        }

        #endregion

        #region IFormattable Members

        string System.IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            return this.value.ToString(format, formatProvider);
        }

        #endregion
    }
}
