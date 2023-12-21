using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace trvotes.Models;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AllMember> AllMembers { get; set; }

    public virtual DbSet<AttendeceType> AttendeceTypes { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<TmpMemberState> TmpMemberStates { get; set; }

    public virtual DbSet<Vote> Votes { get; set; }

    public virtual DbSet<VotingSession> VotingSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:Default");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<AllMember>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsClarified)
                .HasMaxLength(5)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AttendeceType>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TypeValue).HasColumnName("type_value");
        });

        modelBuilder.Entity<Member>(entity =>
        {

            entity.HasKey(x => x.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.JFirstVote).HasColumnName("jFirstVote");
        });

        modelBuilder.Entity<TmpMemberState>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(e => e.MemberId, "IX_TmpMemberStates_MemberId").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Member).WithOne(p => p.TmpMemberState)
                .HasForeignKey<TmpMemberState>(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasIndex(e => e.MemberId, "IX_Votes_MemberId");

            entity.HasIndex(e => e.VotingSessionId, "IX_Votes_VotingSessionId");
            entity.HasKey(x => x.Id);

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Activity).HasColumnName("activity");
            entity.Property(e => e.Jdate).HasColumnName("jdate");

            entity.HasOne(d => d.Member).WithMany(p => p.Votes)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.VotingSession).WithMany(p => p.Votes)
                .HasForeignKey(d => d.VotingSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<VotingSession>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.Jdate).HasColumnName("jdate");
            entity.Property(e => e.Title).HasColumnName("title");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
