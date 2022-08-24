using System;
using System.Collections.Generic;
using System.Drawing;
using EDKv5.Statistics;

namespace EDKv5
{
    public class House
    {
        //constructor
        public House(char key, string name, Color color)
        {
            Key = key;
            Name = name;
            Color = color;
        }

        private Color _color;

        public char Key { get; set; }
        public string Name { get; set; }
        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }
        public int StudentsCount
        {
            get
            {
                Project prj = Project.GetInstance();
                List<Student> stus = prj.GetStudents(this);
                return stus.Count;
            }
        }
        public Student[] Students
        {
            get
            {
                Project prj = Project.GetInstance();
                List<Student> stus = prj.GetStudents(this);
                return stus.ToArray();
            }
        }
        public int Participation
        {
            get
            {
                Project prj = Project.GetInstance();
                ParticipationStatistic stat = Events.ParticipationStatistic;
                House[] hs = prj.Houses;
                int idx = Array.IndexOf(hs, this);
                return stat.House[idx];
            }
        }

        //override
        public override bool Equals(object obj)
        {
            if (obj is House)
                return this == (House)obj;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return Key ^ Name.Length ^ Color.GetHashCode();
        }
    }
}
