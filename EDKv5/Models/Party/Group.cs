using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5
{
    [Flags]
    public enum Group : byte
    {
        None = 0x00,
        FD = 0x01,
        FC = 0x02,
        FB = 0x04,
        FA = 0x08,

        MD = 0x10,
        MC = 0x20,
        MB = 0x40,
        MA = 0x80,

        D = 0x11,
        C = 0x22,
        B = 0x44,
        A = 0x88,

        Male = 0xF0,
        Female = 0x0F,

        Team = 0xFF
    }
    public static class Groups
    {
        public static readonly Group[] All = new Group[]
        {
            Group.MA, Group.MB, Group.MC, Group.MD,
            Group.FA, Group.FB, Group.FC, Group.FD,
        };
        public static Group[] CMode
        {
            get
            {
                return new Group[]
                {
                    Group.MA, Group.MB, Group.MC,
                    Group.FA, Group.FB, Group.FC,
                };
            }
        }

        public static void Convert(ref Group grp, Group to)
        {
            bool bm = (grp & Group.Male) > 0;
            bool bf = (grp & Group.Female) > 0;

            int vto = (int)to;
            vto |= vto << 4;
            vto |= vto >> 4;
            vto &= 0xFF;
            grp = (Group)vto;

            if (bm ^ bf)
            {
                if (bm)
                    grp &= Group.Male;
                else
                    grp &= Group.Female;
            }
        }
    }
}
