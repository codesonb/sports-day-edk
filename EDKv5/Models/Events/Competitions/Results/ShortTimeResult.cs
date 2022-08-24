using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5
{
    class ShortTimeResult : CompetitionResult
    {
        protected override string _ext_result
        {
            get
            {
                return string.Format("{0:00}:{1:00}.{2:00}", h, m, s);
            }
            set
            {
                value = value.Replace(":", "");
                value = value.Replace("\"", "");
                value = value.PadLeft(6, '0');

                h = byte.Parse(value.Substring(0, 2));
                m = byte.Parse(value.Substring(2, 2));
                s = byte.Parse(value.Substring(4, 2));

                if (m > 59) { m -= 60; h++; }
            }
        } // end of result
        public override string ToString()
        {
            return string.Format("{0:00}:{1:00}\"{2:00}", h, m, s);
        }
    }
}
