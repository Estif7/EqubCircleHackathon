namespace EkubCircle.Application.DTOs;

public record RoundDto(
    int Id,
    int RoundNumber,
    string Status,
    decimal Contribution,
    decimal Pot,
    int PaidCount,
    int TotalMembers,
    int ReceiverMembershipId,
    string ReceiverName,
    bool ReceiverHasReceived,
    DateTime OpenedAt,
    DateTime? PaidOutAt,
    decimal? PayoutAmount);

public record MarkPaymentRequest(decimal? Amount, string? ReferenceNote, DateTime? PaidAt);
public record PayoutResponse(int RoundId, int RoundNumber, string ReceiverName, decimal Amount, string Status, string Message);
public record PaymentResponse(int RoundId, int CircleId, int MembershipId, decimal Amount, string Message);
