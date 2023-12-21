using System;
using System.Collections.Generic;

namespace trvotes.Models;

public partial class Vote
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    public int Activity { get; set; }

    public string Jdate { get; set; } = null!;

    public int VotingSessionId { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual VotingSession VotingSession { get; set; } = null!;
}
