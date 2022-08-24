using EDKv5.Protocols;
using System.Collections.Generic;

namespace EDKv5
{
    public interface ICompetitionResultType : IComparer<CompetitionResult>
    {
        CompetitionResult CreateResult();
    }

    public static class ExtendsResultType
    {
        public static CompetitionResult CreateResult(this ICompetitionResultType ori, ILaneSetting laneSetting)
        {
            CompetitionResult comp = ori.CreateResult();
            comp.Lane = laneSetting.Lane;
            comp.Value = laneSetting.Value;
            comp.Rank = laneSetting.Rank;
            comp.State = laneSetting.State;
            return comp;
        }
    }
}
