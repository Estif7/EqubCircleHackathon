using EkubCircle.Domain.Entities;
using EkubCircle.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Circle> Circles => Set<Circle>();
    public DbSet<CircleMembership> CircleMemberships => Set<CircleMembership>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Payout> Payouts => Set<Payout>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.PhoneNumber).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.FaydaFanHash).IsUnique();

        modelBuilder.Entity<Circle>().Property(x => x.ContributionAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Payout>().Property(x => x.Amount).HasPrecision(18, 2);

        modelBuilder.Entity<Circle>()
            .HasOne(x => x.Organizer).WithMany(x => x.OrganizedCircles)
            .HasForeignKey(x => x.OrganizerId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CircleMembership>()
            .HasOne(x => x.Circle).WithMany(x => x.Memberships)
            .HasForeignKey(x => x.CircleId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CircleMembership>()
            .HasOne(x => x.User).WithMany(x => x.Memberships)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CircleMembership>().HasIndex(x => new { x.CircleId, x.UserId }).IsUnique();
        modelBuilder.Entity<CircleMembership>().HasIndex(x => new { x.CircleId, x.PayoutOrder }).IsUnique();

        modelBuilder.Entity<Round>()
            .HasOne(x => x.Circle).WithMany(x => x.Rounds)
            .HasForeignKey(x => x.CircleId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Round>()
            .HasOne(x => x.ReceiverMembership).WithMany(x => x.ReceiverRounds)
            .HasForeignKey(x => x.ReceiverMembershipId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Round>().HasIndex(x => new { x.CircleId, x.RoundNumber }).IsUnique();

        modelBuilder.Entity<Payment>()
            .HasOne(x => x.Round).WithMany(x => x.Payments)
            .HasForeignKey(x => x.RoundId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Payment>()
            .HasOne(x => x.Membership).WithMany(x => x.Payments)
            .HasForeignKey(x => x.MembershipId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Payment>()
            .HasOne(x => x.RecordedByUser).WithMany()
            .HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Payment>().HasIndex(x => new { x.RoundId, x.MembershipId }).IsUnique();

        modelBuilder.Entity<Payout>()
            .HasOne(x => x.Round).WithOne(x => x.Payout)
            .HasForeignKey<Payout>(x => x.RoundId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Payout>()
            .HasOne(x => x.ReceiverMembership).WithMany(x => x.Payouts)
            .HasForeignKey(x => x.ReceiverMembershipId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Payout>()
            .HasOne(x => x.ConfirmedByUser).WithMany()
            .HasForeignKey(x => x.ConfirmedByUserId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OtpVerification>()
            .HasOne(x => x.User).WithMany()
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OtpVerification>().HasIndex(x => new { x.UserId, x.VerifiedAt });
    }
}
