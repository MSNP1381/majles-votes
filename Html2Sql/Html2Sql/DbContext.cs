using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Xml;

namespace Html2Sql
{
    public class DataContext : IdentityUserContext<RegisterModel>
    {
        public DataContext(DbContextOptions options)
            : base(options)
        {
            //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Vote>().Property(b => b.Id).UseIdentityAlwaysColumn();
            builder.Entity<AllMembers>().Property(b => b.Id).UseIdentityAlwaysColumn();
            builder.Entity<Member>().HasOne(e => e.state)
            .WithOne(e => e.Member)
            .HasForeignKey<TmpMemberState>(e => e.MemberId)
            .IsRequired();
            builder.Entity<IdentityUserRole<string>>().HasKey(p => new { p.UserId, p.RoleId });
            builder.Entity<IdentityRole>().HasKey(p => new { p.Id, p.Name });
            builder.Entity<Vote>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            //builder.Entity<Vote>().Property(b => b.Id).HasIdentityOptions().UseIdentityAlwaysColumn();
        }

        public DbSet<Member> Members { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<VotingSession> VotingSessions { get; set; }
        public DbSet<AttendanceTypeTbl> AttendeceTypes { get; set; }
        public DbSet<AllMembers> AllMembers { get; set; }
        public DbSet<TmpMemberState> TmpMemberStates { get; set; }
    }
}
