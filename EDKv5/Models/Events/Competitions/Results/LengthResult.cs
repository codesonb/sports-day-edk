using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5
{
    class LengthResult : CompetitionResult
    {
        protected override string _ext_result
        {
            get
            {
                string tmp = string.Format("{0:0}.{1:00}", h, n);
                double val = Convert.ToDouble(tmp);
                return val.ToString() + "m";
            }
            set
            {
                if (0 == value.Length)
                {
                    h = 0;
                    n = 0;
                    return;
                }
                // ---
                string[] k = value.Split(new char[] { '.' }, 2);
                h = Convert.ToByte(k[0]);

                if (k.Length > 1)
                {
                    while (k[1].Length < 4) { k[1] += "0"; }
                    n = Convert.ToUInt16(k[1]);
                }
                else
                {
                    n = 0;
                }
                
            }
        } // end of result

        public override string ToString()
        {
            return string.Format("{0}.{1}m", h, n);
        }

        //return suppress input
        protected sealed override bool isAllowDecimal() { return true; }
        
        //update sort direction
        public sealed override int CompareTo(CompetitionResult obj)
        {
            return -base.CompareTo(obj);
        }

    }
}
