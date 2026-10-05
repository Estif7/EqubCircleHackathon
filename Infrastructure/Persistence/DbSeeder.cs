using EkubCircle.Domain.Enums;
using EkubCircle.Infrastructure.Services;
using EkubCircle.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var organizer = new User
        {
            FullName = "Demo Organizer",
            Email = "organizer@hackathon.local",
            PhoneNumber = "+251900000001",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Organizer123!"),
            FaydaFanHash = new Sha256FaydaFanHasher().Hash("DEMO-FAN-001"),
            FaydaVerified = true
        };
        var member = new User
        {
            FullName = "Demo Member",
            Email = "member@hackathon.local",
            PhoneNumber = "+251900000002",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Member123!"),
            FaydaFanHash = new Sha256FaydaFanHasher().Hash("DEMO-FAN-002"),
            FaydaVerified = true
        };
        db.Users.AddRange(organizer, member);
        await db.SaveChangesAsync();

        var circle = new Circle
        {
            Name = "Hackathon Demo Ekub",
            ContributionAmount = 1000,
            Frequency = Frequency.Monthly,
            MemberLimit = 4,
            OrganizerId = organizer.Id,
            Status = CircleStatus.Open
        };
        db.Circles.Add(circle);
        await db.SaveChangesAsync();

        db.CircleMemberships.AddRange(
            new CircleMembership { CircleId = circle.Id, UserId = organizer.Id, RoleInCircle = "Organizer + Member", MembershipStatus = MembershipStatus.Active, PayoutOrder = 1 },
            new CircleMembership { CircleId = circle.Id, UserId = member.Id, RoleInCircle = "Member", MembershipStatus = MembershipStatus.Active, PayoutOrder = 2 });
        await db.SaveChangesAsync();
    }
}
