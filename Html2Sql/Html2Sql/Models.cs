using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
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
    public enum EducationLevel
    {
        bachelor,
        master,
        phd,

    }

    public enum BoardType
    {
        President,
        VicePresident,
        SecondVicePresident,
        BoardDirector,
        None,
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
    public class AttendanceTypeTbl
    {
        public int Id { get; set; }
        public int Key { get; set; }
        public string? Value { get; set; }
    }
    public class MemeberDetails
    {
        public ObjectId Id { get; set; }
        public string Url { get; set; }
        public List<BoardType> BoardType { get; set; } = new();
        public List<int> BoardYear { get; set; } = new();
        public string ChooseRegion { get; set; }
        public string FullName { get; set; }
        public string jBirth { get; set; }
        public string religious { get; set; } = "اسلام،شیعه";
        public List<int> History { get; set; }= new() { 11 };
        public DateTime BirthDate
        {
            get
            {
                CultureInfo persianCulture = new CultureInfo("fa-IR");
                try
                {

                return DateTime.ParseExact(jBirth, "yyyy/MM/dd", persianCulture);
                }
                catch
                {
                    return new DateTime();
                }

            }
            set { }
        }
        public string BirthPlace { get; set; }
        public List<Education> Education { get; set; }
        public string ChooseDate { get; set; }
        public string ChooseState { get; set; }
        public int VotesRecived { get; set; }
        public int VotesTotal { get; set; }
        public string jcertified { get; set; }
        public List<Membership> Memberships { get; set; }

    }
    public record Education(EducationLevel Level,string educationName,bool is_graduated);
    //public class MemeberDetailsCommission
    //{
    //    public int CommissionId { get; set; }
    //    public int MemeberDetailsId { get; set; }
    //    public MemeberDetails MemeberDetails { get; set; } = null!;
    //    public Commission Commission { get; set; } = null!;
    //    public int Year { get; set; }
    //    public bool isInCharge { get; set; }
    //}
    public enum MembershipType
    {
        Commission=0,
        Fraction=1,
    
        Friendship=2,
    }

    public class Membership
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? Year { get; set; }
        //public List<MemeberDetailsCommission> Memebers { get; set; }
        public string Url { get; set; }
        public MembershipType Type { get; set; }
        //public int MemeberId { get; set; }
        //[ForeignKey("MemeberId")]
        //public MemeberDetails Memeber { get; set; }
    }

}
