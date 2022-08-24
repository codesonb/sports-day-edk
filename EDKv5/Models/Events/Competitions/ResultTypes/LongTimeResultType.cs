using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDKv5
{
    class LongTimeResultType : ICompetitionResultType
    {
        /* ----- singleton pattern ----- */
        private static LongTimeResultType _instance;
        public static LongTimeResultType GetInstance()
        {
            if (null == _instance) _instance = new LongTimeResultType();
            return _instance;
        }
        /* ----- singleton pattern ----- */
        public int Compare(CompetitionResult x, CompetitionResult y)
        {
            return x.CompareTo(y);
        }

        public CompetitionResult CreateResult()
        {
            return new LongTimeResult();
        }
    }
}
