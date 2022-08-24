using System;

namespace EDKv5
{
    class LengthResultType : ICompetitionResultType
    {
        /* ----- singleton pattern ----- */
        private static LengthResultType _instance;
        public static LengthResultType GetInstance()
        {
            if (null == _instance) _instance = new LengthResultType();
            return _instance;
        }
        /* ----- singleton pattern ----- */

        public int Compare(CompetitionResult x, CompetitionResult y)
        {
            //invert
            return y.CompareTo(x);
        }

        public CompetitionResult CreateResult()
        {
            return new LengthResult();
        }

        public bool IsSuppressInput(string original, char nextChar)
        {
            return _i_is_suppress_input(original, nextChar);
        }
        internal static bool _i_is_suppress_input(string original, char nextChar)
        {
            if (nextChar >= '0' && nextChar <= '9')
                return false;

            if (nextChar == '.')
                return original.Length == 0 || original.Contains(".");

            return true;
        }
    }
}
