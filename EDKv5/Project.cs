using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using EDKv5.Utility.Scanners;
using Data.Serializers;
using System.Windows.Forms;

namespace EDKv5
{
    public class Project
    {
        //events
        public event EventHandler LoadedStudents;

        #region Hard Coded Presets
        static string[] pre_houseNames = new string[] { "Mars", "Mercury", "Jupitar", "Venus" };
        static Color[] pre_houseColor = new Color[] { Color.Red, Color.Blue, Color.LawnGreen, Color.Yellow };
        #endregion

        /*------------ singleton ------------*/
        #region Singleton
        private Project()
        {
            //init year group setting
            CreationDate = DateTime.Now;
            DateTime dref = new DateTime(CreationDate.Year, 12, 31);
            GroupReferenceDate = dref;
        }
        private static Project _instance;
        public static Project GetInstance() { return _instance ?? (_instance = new Project()); }
        #endregion
        /*-----------------------------------*/

        //private fields
        #region Private Fields
        short[] _laneOrder = new short[] { 4, 5, 3, 6, 2, 7, 1, 8 };

        //main data store
        List<House> ls_hs = new List<House>();
        List<Class> ls_cs = new List<Class>();
        HashSet<Student> ls_st = new HashSet<Student>();
        List<Event> ls_ev = new List<Event>();
        SortedList<EventIndex, SortedList<Group, HolderRecord>> ls_hd = new SortedList<EventIndex, SortedList<Group, HolderRecord>>();

        //group reference
        DateTime _grp_ref;

        //path
        string _path;
        #endregion

        // public properties
        #region Public Properties
        //preset event dynamic casual content

        //encapsulated fields
        public DateTime CreationDate { get; }
        public DateTime LastModify { get; private set; }
        public string Path { get { return _path; } }

        //binary state setting
        public bool IsEventConfirmed { get; set; }
        public bool IsJoinLocked { get; set; }

        public Schedule Schedule;
        #endregion

        //custom setting
        #region User Customizable Configs (Supposed to be)
        public string Name { get; set; } = "5th Hong Kong Secondary School Swimming Gala";
        public short[] LaneOrder
        {
            get { return _laneOrder; }
            set
            {
                if (value.Length != 8)
                    throw new ArgumentException("must be array with length 8.");

                bool[] b = new bool[8];
                foreach (int k in value)
                {
                    if (k < 1 || k > 8)
                        throw new ArgumentException("lane index must be 1-8.");
                    if (b[k - 1])
                        throw new ArgumentException("lane number reduplicated.");
                    b[k - 1] = true;
                }
                _laneOrder = value;
            }
        }
        public DateTime GroupReferenceDate
        {
            get { return _grp_ref; }
            set { _grp_ref = value.AddYears(-16); }
        }

        //project year
        public int Year = DateTime.Now.Year;
        #endregion

        #region Update Events
        public void AddEvent(Event ev) { if (!ls_ev.Contains(ev)) ls_ev.Add(ev); }
        public void RemoveEvent(Event ev) { ls_ev.Remove(ev); }
        public void RemoveAllEvents() { ls_ev.Clear(); }
        #endregion

        #region Holder Records
        public Tuple<Group, HolderRecord>[] GetHolders(EventIndex evid)
        {
            SortedList<Group, HolderRecord> slhd;
            if (ls_hd.TryGetValue(evid, out slhd))
            {
                List<Tuple<Group, HolderRecord>> ls = new List<Tuple<Group, HolderRecord>>();

                foreach (KeyValuePair<Group, HolderRecord> kvp in slhd)
                    ls.Add(new Tuple<Group, HolderRecord>(kvp.Key, kvp.Value));

                return ls.ToArray();
            }

            return new Tuple<Group, HolderRecord>[0];
        }
        public HolderRecord GetHolder(EventIndex evid, Group group)
        {
            SortedList<Group, HolderRecord> slhd;
            if (ls_hd.TryGetValue(evid, out slhd))
            {
                HolderRecord hrd;
                slhd.TryGetValue(group, out hrd);
                return hrd;
            }
            return null;
        }
        public void SetHolder(EventIndex evid, Group group, HolderRecord record)
        {
            SortedList<Group, HolderRecord> slhd;
            if (!ls_hd.TryGetValue(evid, out slhd))
            {
                //create new
                slhd = new SortedList<Group, HolderRecord>();
                ls_hd.Add(evid, slhd);
            }

            //set
            if (slhd.ContainsKey(group))
                slhd[group] = record;
            else
                slhd.Add(group, record);

        }
        public void RemoveHolder(EventIndex eventIdx, Group group)
        {
            SortedList<Group, HolderRecord> lsRcd;
            if (ls_hd.TryGetValue(eventIdx, out lsRcd))
                lsRcd.Remove(group);
        }
        #endregion

        #region Object Factory
        public Class CreateClass(int form, char sign) { Class cls = new Class(form, sign); ls_cs.Add(cls); return cls; }
        public House CreateHouse(char id, string name, Color color)
        {
            //set preset
            if (ls_hs.Count < 4 && color == Color.White)
            {
                name = pre_houseNames[ls_hs.Count];
                color = pre_houseColor[ls_hs.Count];
            }

            House hs = new House(id, name, color);
            ls_hs.Add(hs);
            return hs;
        }
        public CompetingGroup[] CreateCompetingGroups()
        {
            List<CompetingGroup> ls = new List<CompetingGroup>();

            int idx = 0;
            foreach (Event ev in ls_ev)
            {
                if (!ev.IsLaneAssigned) { throw new Exception("Events Not Ready"); }

                foreach (Group grp in ev.OpenedGroups)
                    ls.Add(new CompetingGroup(++idx, ev, grp));
            }
            return ls.ToArray();
        }
        #endregion

        #region Try Get Objects
        public bool TryGetClass(string className, out Class cls)
        {
            foreach (Class c in ls_cs)
                if (c.Key == className) { cls = c; return true; }
            cls = null;
            return false;
        }
        public bool TryGetHouse(char houseId, out House house)
        {
            foreach (House h in ls_hs)
                if (h.Key == houseId) { house = h; return true; }
            house = null;
            return false;
        }
        public bool TryGetEvent(string id, out Event @event)
        {
            foreach (Event ev in ls_ev)
                if (ev.ID == id) { @event = ev; return true; }
            @event = null;
            return false;
        }
        public bool TryGetCompeition(int id, out ICompetition competition)
        {
            try
            {
                competition = ProjectCache.GetCompetitionByID(id);
                return true;
            }
            catch
            {
                competition = null;
                return false;
            }
        }
        #endregion

        #region Get Object Instance
        internal Competition _i_getCompetition(int evId, Group group, int compId)
        {
            //start search
            string searchId = evId.ToString("00");
            //search event
            Event trg = null;
            foreach (Event ev in ls_ev)
            {
                if (ev.ID == searchId)
                {
                    trg = ev;
                    break;
                }
            }
            if (null == trg) throw new Exception("Event ID is not found.");
            //search group
            if (Group.None == trg.Contains(group)) throw new Exception("Group is not found.");
            //search competition
            Competition[] cmps = trg._i_getCompetitions(group);
            if (compId < 0 || cmps.Length < compId) throw new Exception("Competition is not found.");

            //response
            return cmps[compId - 1];
        }
        #endregion

        #region Get Object Lists
        public Student[] Students { get { return ls_st.ToArray(); } }
        public Class[] Classes { get { return ls_cs.ToArray(); } }
        public House[] Houses { get { return ls_hs.ToArray(); } }
        public Event[] Events { get { return ls_ev.ToArray(); } }
        public Event[] NonRelayEvents
        {
            get
            {
                EventIndex[] relays = EDKv5.Events.Relay;
                List<Event> evs = new List<Event>();
                foreach (Event ev in ls_ev)
                {
                    EventIndex eid = (EventIndex)Enum.Parse(typeof(EventIndex), ev.ID);
                    if (eid < EventIndex.Custom && !relays.Contains(eid)) evs.Add(ev);
                }
                return evs.ToArray();
            }
        }

        #region Class with Students
        public Dictionary<Class, List<Student>> ClassStudents
        {
            get
            {
                //output structure
                Dictionary<Class, List<Student>> ls = new Dictionary<Class, List<Student>>();

                //loop through all students
                foreach (Student stu in ls_st)
                {
                    //check class exists
                    List<Student> lsStu;
                    ls.TryGetValue(stu.Class, out lsStu);

                    //return
                    if (null == lsStu)
                    {
                        lsStu = new List<Student>();
                        ls.Add(stu.Class, lsStu);
                    }

                    lsStu.Add(stu);
                }

                return ls;
            }
        }
        #endregion

        #region Students by Criteria
        public List<Student> GetStudents(Class cls)
        {
            List<Student> lsOut = new List<Student>();
            foreach (Student stu in ls_st)
            {
                if (cls == stu.Class)
                    lsOut.Add(stu);
            }
            return lsOut;
        }
        public List<Student> GetStudents(House hs)
        {
            List<Student> lsOut = new List<Student>();
            foreach (Student stu in ls_st)
            {
                if (hs == stu.House)
                    lsOut.Add(stu);
            }
            return lsOut;
        }
        public List<Student> GetStudents(Group grp)
        {
            List<Student> lsOut = new List<Student>();
            foreach (Student stu in ls_st)
                if (grp == stu.Group)
                    lsOut.Add(stu);
            return lsOut;
        }
        #endregion

        #endregion

        #region Manual Set Instance
        public void AddHouse(House house)
        {
            if (!ls_hs.Contains(house))
                ls_hs.Add(house);
        }
        public void AddClass(Class @class)
        {
            if (!ls_cs.Contains(@class))
                this.ls_cs.Add(@class);
        }
        public void AddStudent(Student student)
        {
            if (!ls_st.Contains(student))
            {
                ls_st.Add(student);
                student.UpdateGroup(this);
                AddClass(student.Class);
            }
        }
        #endregion

        #region Untility Properties
        public int StudentsCount { get { return ls_st.Count; } }
        #endregion

        #region User Input Excel
        public void LoadStudents(string path, ExcelScanner<Student> scanner)
        {
            try
            {
                //read
                ls_st = new HashSet<Student>(scanner.ReadDataset(path));

                //calculate group
                Student[] students = ls_st.ToArray();
                foreach (Student stu in students)
                    stu.UpdateGroup(this);

                //clear original data
                Event[] events = ls_ev.ToArray();
                foreach (Event ev in events)
                    ev.ClearParticipants();

                if (null != this.LoadedStudents)
                    this.LoadedStudents(this, EventArgs.Empty);

            }
            catch (Exception ex)
            {
                //not implemented for error handling
                Console.WriteLine(ex.GetType().Name);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                throw ex;   //re-z
            }
        }
        public void LoadHolders(string path, ExcelScanner<HolderRecord> scanner)
        {
            try
            {
                List<HolderRecord> lsHr = scanner.ReadDataset(path);

                foreach (HolderRecord hr in lsHr)
                {
                    SortedList<Group, HolderRecord> lsGhr;
                    if (!ls_hd.TryGetValue(hr.Event, out lsGhr))
                    {
                        lsGhr = new SortedList<Group, HolderRecord>();
                        ls_hd.Add(hr.Event, lsGhr);
                    }

                    if (!lsGhr.ContainsKey(hr.Group))
                        lsGhr.Add(hr.Group, hr);
                    else
                        lsGhr[hr.Group] = hr;
                }
            }
            catch (Exception ex)
            {
                //not implemented for error handling
                Console.WriteLine(ex.GetType().Name);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                throw ex;   //re-throw
            }
        }
        #endregion

        #region Save Load Project
        public void Save() { using (Stream stream = File.OpenWrite(_path)) Save(stream); }  //pass to stream
        public void Save(string path)   //pass to stream
        {
            this._path = path;
            using (Stream stream = File.OpenWrite(path))
                Save(stream);
        }
        public static Project Load(string path)
        {
            using (Stream stream = File.OpenRead(path))
            {
                Project prj = Load(stream);
                prj._path = path;
                return prj;
            }
        }
        public void Save(Stream stream)
        {
            LastModify = DateTime.Now;
            ISerializer serializer = BinarySerializer.GetInstance();
            serializer.Serialize(stream, this);
        }
        public static Project Load(Stream stream)
        {
            ISerializer serializer = BinarySerializer.GetInstance();
            return (Project)serializer.Deserialize(stream);
        }
        public static void Set(Project project)
        {
            _instance = project;
        }
        #endregion
    }

}
