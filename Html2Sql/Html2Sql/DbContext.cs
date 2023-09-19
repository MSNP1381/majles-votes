﻿using Microsoft.EntityFrameworkCore;

namespace Html2Sql
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;
        public string DbPath { get; }

        public DataContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "data.db");
        }



        public DbSet<Member> Members { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<VotingSession> VotingSessions { get; set; }
        public DbSet<AttendanceTypeTbl> AttendeceTypes { get; set; }
    }
}
