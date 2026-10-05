using EkubCircle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EkubCircle.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Circle> Circles { get; }
    DbSet<CircleMembership> CircleMemberships { get; }
    DbSet<Round> Rounds { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Payout> Payouts { get; }
    DbSet<OtpVerification> OtpVerifications { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
