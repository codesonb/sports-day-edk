using Data.Piers;
using EDKv5.MonitorServices;
using System.Collections.Generic;

namespace EDKv5.Protocols
{
    public class CompetitionResponse : IResponse
    {
        //constructor
        internal CompetitionResponse(ICompetition competition)
        {
            Project prj = Project.GetInstance();

            //convert data
            CompetitionID = competition.ID();
            CompetitionName = string.Format("{0} - Heat {1}", competition.Event.Name, competition.Index);
            Group = competition.Group;
            ResultType = competition.Event.ResultType;
            IsField = competition.Event.IsField;

            this.Lanes = competition.CreateLaneSettings();

            if (competition.Event.NeedLaneAssignment)
            {
                short[] laneOrder = prj.LaneOrder;
                for (int i = 0; i < Lanes.Length; i++)
                    ((LaneSetting)Lanes[i]).Lane = laneOrder[Lanes[i].Lane - 1];
            }
        }

        //fields
        public int CompetitionID { get; private set; }
        public string CompetitionName { get; private set; }
        public Group Group { get; private set; }
        public ICompetitionResultType ResultType { get; private set; }
        public bool IsField { get; private set; }
        public ILaneSetting[] Lanes { get; private set; }

        //function
        public void Process()
        {
            ResponseMediator rM = ResponseMediator.Instance;
            rM.onGotCompetitionDetail(this);
        }
    }
}
