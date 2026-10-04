using EkubCircle.Application.DTOs;

namespace EkubCircle.Application.Interfaces;

public interface IRoundService
{
    Task<RoundDto> GetCurrentAsync(int userId, int circleId);
    Task<IReadOnlyList<RoundDto>> GetRoundsAsync(int userId, int circleId);
    Task<IReadOnlyList<HistoryItemDto>> GetHistoryAsync(int userId, int circleId);
    Task<PaymentResponse> MarkPaymentAsync(int userId, int roundId, int membershipId, MarkPaymentRequest request);
    Task<PayoutResponse> PayoutAsync(int organizerId, int circleId);
}
