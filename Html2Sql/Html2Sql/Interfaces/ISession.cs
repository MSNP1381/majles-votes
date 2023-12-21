using Html2Sql;
using trvotes.Models;

namespace trvotes.Interfaces
{
    public class IAllMembers : AllMember
    {
        public int Absence { get; set; } = -1;
        public int NonParticipation { get; set; } = -1;
        public int Against { get; set; } = -1;
        public int Favor { get; set; } = -1;
        public int Abstaining { get; set; } = -1;
        public string jFirstVote { get; set; } = "0001/01/01";
    }
}
