using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5
{
    public sealed class Student : Participant
    {
        //construtor
        public Student(string sid, string name, Class cls, int clsno, House house, char gender, DateTime dob)
        {
            _sid = sid;
            Name = name;
            Class = cls;
            ClassNo = clsno;
            House = house;

            Group = gender == 'F' ? Group.FD : Group.MD;
            this.dob = dob;
        }

        //fields
        DateTime dob;
        string _sid;

        //public fileds
        public bool IsPotentialAthlet;

        //properties
        public override string ID { get { return _sid; } }
        public override string Name { get; }
        public Class Class { get; }
        public int ClassNo { get; }
        public override string ClassName { get { return Class.Key; } }

        //functions
        public void UpdateGroup(Project project)
        {
            Group grp = this.Group;
            DateTime dref = project.GroupReferenceDate;
            if (dob < dref)
                Groups.Convert(ref grp, Group.A);
            else if (dob < (dref = dref.AddYears(2)))
                Groups.Convert(ref grp, Group.B);
            else if (dob < (dref = dref.AddYears(2)))
                Groups.Convert(ref grp, Group.C);

            this.Group = grp;
        }

    }
}
