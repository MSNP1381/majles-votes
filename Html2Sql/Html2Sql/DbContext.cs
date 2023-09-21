using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection.Emit;

namespace Html2Sql
{
    public class DataContext : IdentityUserContext<RegisterModel>
    {

        public DataContext(DbContextOptions options):base(options)
        {
                
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<IdentityUserRole<string>>().HasKey(p => new { p.UserId, p.RoleId });
            builder.Entity<IdentityRole>().HasKey(p => new { p.Id, p.Name });


        }


        public DbSet<Member> Members { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<VotingSession> VotingSessions { get; set; }
        public DbSet<AttendanceTypeTbl> AttendeceTypes { get; set; }
    }
}
