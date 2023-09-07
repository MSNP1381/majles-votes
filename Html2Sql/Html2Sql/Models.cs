using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Html2Sql
{
    public enum AttendanceType
    {
        absence,
        nonParticipation,
        against,
        favor,
        abstaining
    }
    public class Member
    {
        public int Id { get; set; }
        public int MemId { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public string Region { get; set; }
        public string ImageUrl { get; set; }
        public string? Image { get; set; }
        public virtual List<Vote> Votes { get; set; }
    }
    public class Vote
    {
        public int Id { get; set; }

        public int MemberId { get; set; }
        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }
        public AttendanceType activity { get; set; }
        public string jdate { get; set; }
        public DateTime Date
        {
            get
            {
                CultureInfo persianCulture = new CultureInfo("fa-IR");
                DateTime persianDateTime = DateTime.ParseExact(jdate, "yyyy/MM/dd", persianCulture);
                return persianDateTime;
            }
            set
            {

            }
        }
        public int VotingSessionId { get; set; }
        [ForeignKey("VotingSessionId")]
        public VotingSession VotingSession { get; set; }
    }
    public class VotingSession
    {
        public int Id { get; set; }
        public string title { get; set; }
        public int Against { get; set; }
        public int Favor { get; set; }
        public int Abstaining { get; set; }
        public string jdate { get; set; }

        public virtual List<Vote> Votes { get; set; }
        public DateTime Date
        {
            get
            {
                CultureInfo persianCulture = new CultureInfo("fa-IR");
                DateTime persianDateTime = DateTime.ParseExact(jdate, "yyyy/MM/dd", persianCulture);
                return persianDateTime;
            }
            set
            {

            }
        }
    }

}
