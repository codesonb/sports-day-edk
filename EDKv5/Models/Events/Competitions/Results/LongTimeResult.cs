using System;

namespace EDKv5
{
    class LongTimeResult : CompetitionResult
    {
        protected override string _ext_result
        {
            get
            {
                return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
            }
            set
            {
                value = value.Replace(":", "");
                value = value.PadLeft(6, '0');

                h = Convert.ToByte(value.Substring(0, 2));
                m = Convert.ToByte(value.Substring(2, 2));
                s = Convert.ToByte(value.Substring(4, 2));

                if (s > 59) { s -= 60; m++; }
                if (m > 59) { m -= 60; h++; }
            }
        } // end of result
        public override string ToString()
        {
            return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
        }
    }
}
