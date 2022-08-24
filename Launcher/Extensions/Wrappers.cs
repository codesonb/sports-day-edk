using System.Collections.Generic;
using EDKv5;

namespace Launcher.Wrappers
{
    class CompetitionStackCollection : Stack<ICompetition>
    {
        public CompetitionStackCollection(IEnumerable<ICompetition> competition) : base(competition) { }

        public ICompetition PeekItem
        {
            get { return this.Peek(); }
        }
    }
}
