using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace trvotes.Models;

public partial class VotingSession
{
    [NotMapped]
    public DateTime Date
    {
        get
        {
            CultureInfo persianCulture = new CultureInfo("fa-IR");
            DateTime persianDateTime = DateTime
                .ParseExact(Jdate, "yyyy/MM/dd", persianCulture)
                .ToUniversalTime();
            return persianDateTime;
        }
    }

}

