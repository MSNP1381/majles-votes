using System;
using System.Collections.Generic;

namespace trvotes.Models;

public partial class Member
{
    public int Id { get; set; }

    public int MajCode { get; set; }

    public string Name { get; set; } = null!;

    public string Family { get; set; } = null!;

    public string Region { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public string? Image { get; set; }

    public string? JFirstVote { get; set; }

    public virtual TmpMemberState? TmpMemberState { get; set; }

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
