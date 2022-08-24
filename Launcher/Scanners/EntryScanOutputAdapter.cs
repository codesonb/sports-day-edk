using EDKv5;
using EDKv5.Utility.Scanners;
using System;
using System.Collections.Generic;
using System.Text;

namespace Launcher.Scanners
{
    class EntryScanOutputAdapter : IScanOutput
    {
        // fields
        Project project;
        IEnumerator<Student> stus;
        Event[] evs;
        List<Tuple<Student, Event>> _failLog = new List<Tuple<Student, Event>>();

        // property
        public Tuple<Student, Event>[] FailJoin { get { return _failLog.ToArray(); } }

        //initialization before use
        public void Init(Project project)
        {
            this.project = project;
            evs = this.project.NonRelayEvents;
            _failLog.Clear();
        }

        // functions
        public int GetColumnCount()
        {
            return evs.Length;
        }

        public bool Next() { return stus.MoveNext(); }

        public bool SetCode(byte[] code)
        {
            string codeStr = Encoding.ASCII.GetString(code, 1, code.Length - 1);

            string clsStr = codeStr.Substring(1, 2);
            int yr = int.Parse(codeStr.Substring(3, 4));

            //analyze valid code
            if (codeStr[0] != 'C')
                throw new ArgumentException("Invalid Code");

            //check year
            if (yr != project.Year)
                throw new InvalidOperationException("File does not match the current project");

            //get class
            Class cls;
            if (!project.TryGetClass(clsStr, out cls))
                throw new InvalidOperationException("Class does not exist");
            else
                stus = project.GetStudents(cls).GetEnumerator();

            return true;
        }

        public int SetRow(bool[] cols)
        {
            int err = 0;
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i])
                {
                    try {
                        if (!evs[i].Join(stus.Current))
                        {
                            _failLog.Add(new Tuple<Student, Event>(stus.Current, evs[i]));
                            throw new Exception("Event is not opened for group of student (" + stus.Current.Name + ").");
                        }
                    }
                    catch { err++; }
                }
            }
            return err;
        }
    }
}
