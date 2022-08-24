using System;

using EDKv5.MonitorServices;

using Data.Piers;

namespace EDKv5.Protocols
{
    public class RequestCompetitionCommand : ICommand
    {
        //constructor
        public RequestCompetitionCommand(int competitionId)
        {
            this.competitionId = competitionId;
        }

        //request fields
        int competitionId;

        //execution commands
        public IResponse Execute()
        {
            Project prj = Project.GetInstance();
            ICompetition comp;
            if (prj.TryGetCompeition(competitionId, out comp))
                return new CompetitionResponse(comp);
            else
                return new FailResponse(this, "Competition does not exist.");
        }

    }
}
