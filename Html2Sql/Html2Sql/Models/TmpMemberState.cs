using System;
using System.Collections.Generic;

namespace trvotes.Models;

public partial class TmpMemberState
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    public int Absence { get; set; }

    public int NonParticipation { get; set; }

    public int Against { get; set; }

    public int Favor { get; set; }

    public int Abstaining { get; set; }

    public virtual Member Member { get; set; } = null!;
}
