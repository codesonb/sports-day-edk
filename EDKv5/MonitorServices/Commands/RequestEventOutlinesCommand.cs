using System;
using System.Collections.Generic;

using Data.Piers;
using EDKv5.SchedulerService;

namespace EDKv5.Protocols
{
    public class RequestTodayEventOutlinesCommand : ICommand
    {

        public IResponse Execute()
        {
            Project prj = Project.GetInstance();

            //List<EventOutline> ls_tmp_eo = new List<EventOutline>();
            Dictionary<string, EventOutline> dic_tmp_eo = new Dictionary<string, EventOutline>();
            try
            {
                // get today
                var days = prj.Schedule.Days;
                var now = DateTime.Now;
                int i = -1;
                while (++i < days.Length - 1)
                {
                    if (days[i].Start > now)
                    { break; }
                }
                var today = days[i];

                foreach (ICompetingPeriod period in today.GetCompetingPeriods())
                {
                    foreach (Cell cell in period.Cells)
                    {
                        var cmpGrp = (CompetingGroup)cell.Data;
                        var ev = cmpGrp.Event;
                        EventOutline outline;
                        if (!dic_tmp_eo.TryGetValue(ev.ID, out outline))
                        {
                            outline = new EventOutline();
                            outline.ID = ev.ID;
                            outline.EventName = ev.Name;
                            outline.IsField = ev.IsField;
                            dic_tmp_eo.Add(ev.ID, outline);
                        }

                        var comps = ev.GetCompetitions(cmpGrp.Group);

                        List<CompetitionOutline> ls_tmp_co = new List<CompetitionOutline>();
                        foreach (ICompetition icmp in comps)
                        {
                            CompetitionOutline compOutline = new CompetitionOutline();
                            compOutline.CompGroup = cmpGrp.Group;
                            compOutline.CompID = icmp.ID();
                            ls_tmp_co.Add(compOutline);
                        }

                        outline.AddCompetitions(ls_tmp_co);
                    }
                }

                EventOutline[] eoarr = new EventOutline[dic_tmp_eo.Count];
                dic_tmp_eo.Values.CopyTo(eoarr, 0);
                return new EventOutlinesResponse(eoarr);
            }
            catch (Exception ex)
            {
                return new FailResponse(this, ex.Message);
            }

        }
    }
}
