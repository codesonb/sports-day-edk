using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDKv5.Statistics
{
    public struct ParticipationStatistic
    {
        public int Total;
        public int[] Group;
        public int[] House;
        public int[] Class;

        public static ParticipationStatistic operator +(ParticipationStatistic p1, ParticipationStatistic p2)
        {
            ParticipationStatistic rtn = p1;
            rtn.Total += p2.Total;
            for (int i = 0; i < p1.Group.Length; i++) p1.Group[i] += p2.Group[i];
            for (int i = 0; i < p1.House.Length; i++) p1.House[i] += p2.House[i];
            for (int i = 0; i < p1.Class.Length; i++) p1.Class[i] += p2.Class[i];
            return rtn;
        }
    }
}
