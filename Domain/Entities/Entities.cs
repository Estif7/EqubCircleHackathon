using EkubCircle.Domain.Enums;

namespace EkubCircle.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FaydaFanHash { get; set; }
    public bool FaydaVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Circle> OrganizedCircles { get; set; } = new List<Circle>();
    public ICollection<CircleMembership> Memberships { get; set; } = new List<CircleMembership>();
}

public class Circle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal ContributionAmount { get; set; }
    public Frequency Frequency { get; set; }
    public int MemberLimit { get; set; }
    public CircleStatus Status { get; set; } = CircleStatus.Open;
    public int OrganizerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User Organizer { get; set; } = null!;
    public ICollection<CircleMembership> Memberships { get; set; } = new List<CircleMembership>();
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}

public class CircleMembership
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public int UserId { get; set; }
    public string RoleInCircle { get; set; } = "Member";
    public int? PayoutOrder { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public MembershipStatus MembershipStatus { get; set; } = MembershipStatus.Active;
    public Circle Circle { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Round> ReceiverRounds { get; set; } = new List<Round>();
    public ICollection<Payout> Payouts { get; set; } = new List<Payout>();
}

public class Round
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public int RoundNumber { get; set; }
    public int ReceiverMembershipId { get; set; }
    public RoundStatus Status { get; set; } = RoundStatus.Open;
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidOutAt { get; set; }
    public decimal? PayoutAmount { get; set; }
    public Circle Circle { get; set; } = null!;
    public CircleMembership ReceiverMembership { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Payout? Payout { get; set; }
}

public class Payment
{
    public int Id { get; set; }
    public int RoundId { get; set; }
    public int MembershipId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string? ReferenceNote { get; set; }
    public int RecordedByUserId { get; set; }
    public Round Round { get; set; } = null!;
    public CircleMembership Membership { get; set; } = null!;
    public User RecordedByUser { get; set; } = null!;
}

public class Payout
{
    public int Id { get; set; }
    public int RoundId { get; set; }
    public int ReceiverMembershipId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public int ConfirmedByUserId { get; set; }
    public PayoutStatus Status { get; set; } = PayoutStatus.Completed;
    public Round Round { get; set; } = null!;
    public CircleMembership ReceiverMembership { get; set; } = null!;
    public User ConfirmedByUser { get; set; } = null!;
}

public class OtpVerification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string OtpHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int Attempts { get; set; }
    public User User { get; set; } = null!;
}
