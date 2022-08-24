using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDKv5
{
    class ShortTimeResultType : ICompetitionResultType
    {
        /* ----- singleton pattern ----- */
        private static ShortTimeResultType _instance;
        public static ShortTimeResultType GetInstance()
        {
            if (null == _instance) _instance = new ShortTimeResultType();
            return _instance;
        }
        /* ----- singleton pattern ----- */

        public int Compare(CompetitionResult x, CompetitionResult y)
        {
            return x.CompareTo(y);
        }

        public CompetitionResult CreateResult()
        {
            return new ShortTimeResult();
        }
    }
}