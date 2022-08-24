using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5
{
    public class HouseTeam : Participant
    {
        private static int _cnt = 1;
        public HouseTeam(House house)
        {
            House = house;
            ID = "T-" + (_cnt++).ToString("00");
        }

        List<Student> _members = new List<Student>();

        public override string ID { get; }
        public override string Name { get { return House.Name; } }
        public override string ClassName { get { return "T-"; } }

        public Student[] Members { get { return _members.ToArray(); } }
    }
}
