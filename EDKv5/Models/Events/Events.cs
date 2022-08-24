using System.Collections.Generic;
using System.IO;
using EDKv5.Statistics;

namespace EDKv5
{
    public static class Events
    {
        private static string[] _ev_names = GetDefaultNames();

        public static void LoadNames(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                Queue<string> lines = new Queue<string>();
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line.Length > 0)
                        lines.Enqueue(line);
                }
                _ev_names = lines.ToArray();
            }
        }

        public static string[] GetNames()
        {
            return _ev_names;
        }
        public static string[] GetDefaultNames()
        {
            return new string[]
            {
                "100m", "200m", "400m", "800m", "1500m", "100m Hurdle", "110m Hurdle",
                "4x100m Relay", "4x400m Relay",
                "High Jump", "Pole Vault",
                "Long Jump", "Triple Jump",
                "Short Put", "Discus", "Javelin",
                "Butterfly 50m",  "Backstroke 50m",  "Breaststroke 50m",  "Freestyle 50m",  "Medley 50m",
                "Butterfly 100m", "Backstroke 100m", "Breaststroke 100m", "Freestyle 100m", "Medley 100m",
                "Butterfly 200m", "Backstroke 200m", "Breaststroke 200m", "Freestyle 200m", "Medley 200m",
                "4 Styles 4x100m Relay", "4 Styles 4x200m Relay",
            };
        }

        public static readonly EventIndex[] Relay = new EventIndex[]
        {
            EventIndex.Relay4x100,
            EventIndex.Relay4x400,
            EventIndex.MedleyRelay4x100,
            EventIndex.MedleyRelay4x200,
        };

        public static ParticipationStatistic ParticipationStatistic
        {
            get
            {
                Project project = Project.GetInstance();
                Event[] evs = project.NonRelayEvents;

                //first
                if (evs.Length > 0)
                {
                    ParticipationStatistic stat = evs[0].ParticipationStatistic;
                    for (int i = 1; i < evs.Length; i++)
                        stat += evs[i].ParticipationStatistic;
                    return stat;
                }
                else
                {
                    ParticipationStatistic stat = new ParticipationStatistic();
                    stat.Group = new int[Groups.All.Length];
                    stat.House = new int[project.Houses.Length];
                    stat.Class = new int[project.Classes.Length];
                    return stat;
                }

            }
        } //--- end participation statistic

        public static CompetitionResult CreateResult(EventIndex eventIndex, string resultString)
        {
            Event ev = Event.Create((int)eventIndex, "tmp");
            ICompetitionResultType resultType = ev.ResultType;

            CompetitionResult result = resultType.CreateResult(null);
            result.Result = resultString;

            return result;
        }

    } // end static class Events
}
