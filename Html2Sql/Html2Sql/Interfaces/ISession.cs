using Html2Sql;

namespace trvotes.Interfaces
{
    public class IAllMembers : AllMembers

    {
        public int Absence { get; set; } = -1;
        public int NonParticipation { get; set; } = -1;
        public int Against { get; set; } = -1;
        public int Favor { get; set; } = -1;
        public int Abstaining { get; set; } = -1;
        public string jFirstVote { get; set; } = "0001/01/01";

    }
}
