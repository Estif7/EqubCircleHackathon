namespace EkubCircle.Application.DTOs;

public record DashboardNextCircleDto(
    int CircleId,
    string CircleName,
    decimal ContributionAmount,
    string Frequency,
    int RoundNumber,
    string ReceiverName,
    bool Ready,
    string Status,
    string Message);

public record DashboardSummaryDto(
    DashboardNextCircleDto? NextPaymentCircle,
    DashboardNextCircleDto? NextPayoutCircle);
