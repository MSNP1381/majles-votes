using System;
using System.Collections.Generic;

namespace trvotes.Models;

public partial class VotingSession
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int Against { get; set; }

    public int Favor { get; set; }

    public int Abstaining { get; set; }

    public string Jdate { get; set; } = null!;

    public int GroupId { get; set; }

    public virtual List<Vote> Votes { get; set; } = new List<Vote>();
}
