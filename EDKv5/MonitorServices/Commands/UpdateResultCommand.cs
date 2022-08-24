using System;
using System.Collections.Generic;

using Data.Piers;
using EDKv5.MonitorServices;

namespace EDKv5.Protocols
{
    public class UpdateResultCommand : ICommand
    {
        //constructor
        public UpdateResultCommand(int id, CompetitionResult[] results)
        {
            this.compId = id;
            this.results = results;
        }

        //request fields
        int compId;
        CompetitionResult[] results;

        //execution commands
        public IResponse Execute()
        {
            Project prj = Project.GetInstance();
            ICompetition icomp;
            if (prj.TryGetCompeition(compId, out icomp))
            {
                try
                {
                    Competition comp = (Competition)icomp;
                    comp._i_updateResults(results);

                    // check display state
                    var relComps = comp.Event.GetCompetitions(comp.Group);
                    var state = computeCompletionState(relComps);

                    //----------------------------------------------------------------------------------
                    // start of notification
                    var monitor = MonitorMediator.Instance;

                    // call for competition completion
                    monitor.onCompetitionStateUpdated(new CompetitionStateUpdatedEventArgs(comp, state));

                    // call for breaking records
                    // - try to get back the event id ( custom events should not have records
                    EventIndex evid;
                    if (Enum.TryParse(comp.Event.ID, out evid))
                    {
                        var holder = prj.GetHolder(evid, comp.Group);
                        if (holder != null)          // <~ if this is a new event we do not have this record, skip it
                        {
                            foreach (CompetitionResult result in results)
                            {
                                if (holder.Result.CompareTo(result) > 0)
                                    monitor.onRecordBreaked(new RecordBreakedEventArgs(comp, result.Participant, result));
                            }
                        }
                    }

                    // call for full event accomplishment
                    if (EventCompletionState.Full == state)
                        monitor.onEventStateUpdated(new EventUpdatedEventArgs(comp.Group, comp.Event));
                }
                catch
                {
                    return new FailResponse(this, "Unknow Error is occured.");
                }
                return null;
            }
            else
            {
                return new FailResponse(this, "Competition ID is not found.");
            }
        } // end IResponse ICommand.Execute()


        private EventCompletionState computeCompletionState(ICompetition[] relComps)
        {
            var remain = relComps.Length;
            foreach (var relComp in relComps)
            {
                if (relComp.IsCompleted)
                {
                    if (!relComp.IsRankMatched) { return EventCompletionState.WaitRank; }
                    remain--;
                }
            }

            if (remain == relComps.Length)
                return EventCompletionState.None;
            else if (remain > 0)
                return EventCompletionState.Partial;
            else
                return EventCompletionState.Full;
        }

    } // end class
}
