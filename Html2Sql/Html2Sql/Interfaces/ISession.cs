using Html2Sql;

namespace trvotes.Interfaces
{
    public class IVoteSession
    {
        public VotingSession VotingSession { get; set; }
        public IEnumerable<Vote> Votes { get; set; }
    }
}
