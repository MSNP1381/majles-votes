using Html2Sql;
using Html2Sql;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Html2Sql
{
    public class Base
    {
        [Key]
        public int Id { get; set; }
        public DateTime EditTime { get; set; } = DateTime.UtcNow;
    }
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

    public class Member:Base
    {

        public int MemId { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public string Region { get; set; }
        public string ImageUrl { get; set; }
        public string? Image { get; set; }
        public string? jFirstVote { get; set; }
        public DateTime FirstVote
        {
            get
            {
                try
                {
                    CultureInfo persianCulture = new CultureInfo("fa-IR");
                    DateTime persianDateTime = DateTime
                        .ParseExact(jFirstVote, "yyyy/MM/dd", persianCulture)
                        .ToUniversalTime();
                    return persianDateTime;
                }
                catch
                {
                    return new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime();
                }
            }
            set { }
        }

        public virtual List<Vote> Votes { get; set; }
        public virtual TmpMemberState state { get; set; }
    }

    public class Token
    {
        public string Value { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class Vote:Base
    {


        public int MemberId { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [JsonIgnore]
        public AttendanceType activity { get; set; }

        [NotMapped]
        public string ActivityName { get; set; }
        public string jdate { get; set; }
        public DateTime Date
        {
            get
            {
                CultureInfo persianCulture = new CultureInfo("fa-IR");
                DateTime persianDateTime = DateTime
                    .ParseExact(jdate, "yyyy/MM/dd", persianCulture)
                    .ToUniversalTime();
                return persianDateTime;
            }
            set { }
        }
        public int VotingSessionId { get; set; }

        [ForeignKey("VotingSessionId")]
        public virtual VotingSession VotingSession { get; set; }
    }

    public class VotingSession:Base
    {
  
        public string title { get; set; }
        public int Against { get; set; }
        public int Favor { get; set; }
        public int Abstaining { get; set; }
        public string jdate { get; set; }
        public int group_id { get; set; }
        public virtual List<Vote> Votes { get; set; }
        public DateTime Date
        {
            get
            {
                CultureInfo persianCulture = new CultureInfo("fa-IR");
                DateTime persianDateTime = DateTime
                    .ParseExact(jdate, "yyyy/MM/dd", persianCulture)
                    .ToUniversalTime();
                return persianDateTime;
            }
            set { }
        }
    }

    public class AttendanceTypeTbl:Base
    {
        public int Id { get; set; }
        public string? type_value { get; set; }
    }

    [NotMapped]
    public class MemeberDetails
    {
        public int MemId { get; set; }
        public ObjectId Id { get; set; }
        public string Url { get; set; }
        public List<BoardMember> BoardHist { get; set; } = new();
        public string ChooseRegion { get; set; }
        public string FullName { get; set; }
        public string jBirth { get; set; }
        public string Religion { get; set; } = "اسلام،شیعه";
        public List<int> History { get; set; } = new() { 11 };
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
        public List<Education> Educations { get; set; }
        public string ChooseDate { get; set; }
        public string ChooseState { get; set; }
        public int VotesRecived { get; set; }
        public int VotesTotal { get; set; }
        public string jcertified { get; set; }
        public List<Membership> Memberships { get; set; }
        public float VotePercent
        {
            get
            {
                try
                {
                    return VotesRecived / VotesTotal;
                }
                catch
                {
                    return 0;
                }
            }
            set { }
        }
        public List<News_Speeches> News { get; set; }
        public List<News_Speeches> Speeches { get; set; }
    }

    public class Education
    {
        public EducationLevel Level;
        public string educationName;
        public bool is_graduated;
        public string EducationLevelName
        {
            get { return Enum.GetName(typeof(EducationLevel), this.Level); }
            set { }
        }
    }

    public enum MembershipType
    {
        Commission = 0,
        Fraction = 1,

        Friendship = 2,
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

    public class EnumVal
    {
        public string Name { get; set; }
        public Dictionary<int, string> Value { get; set; }
    }

    [NotMapped]
    public class BoardMember
    {
        public BoardType BoardType { get; set; }
        public int BoardYear { get; set; }

        public string? BoardTypeName
        {
            get { return Enum.GetName(typeof(BoardType), this.BoardType); }
            set { }
        }
    }

    public class AllMembers : Base
    {
        public int MemId { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public string Region { get; set; }
        public string ImageUrl { get; set; }
        public bool IsClarified { get; set; }
    }
    public class News_Speeches
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string jDate { get; set; }
        public string Desc { get; set; }
        public DateTime Date
        {
            get
            {
                try
                {
                    CultureInfo persianCulture = new CultureInfo("fa-IR");
                    DateTime persianDateTime = DateTime.ParseExact(
                        jDate,
                        "dd MMMM yyyy",
                        persianCulture
                    );
                    return persianDateTime;
                }
                catch
                {
                    return new DateTime(1970, 1, 1);
                }
            }
            set { }
        }
    }
    public class TmpMemberState:Base
    {

        public int MemberId { get; set; }
        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }
        public int Absence { get; set; } = 0;
        public int NonParticipation { get; set; } = 0;
        public int Against { get; set; } = 0;
        public int Favor { get; set; } = 0;
        public int Abstaining { get; set; } = 0;
    }
    public class RegisterModel : IdentityUser { }
}
