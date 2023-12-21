using System;
using System.Collections.Generic;

namespace trvotes.Models;

public partial class AllMember
{
    public int Id { get; set; }

    public int MajCode { get; set; }

    public string Name { get; set; } = null!;

    public string Family { get; set; } = null!;

    public string Region { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public char IsClarified { get; set; } = 'f';
}
