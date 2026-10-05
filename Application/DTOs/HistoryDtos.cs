namespace EkubCircle.Application.DTOs;

public record HistoryItemDto(
    int RoundNumber,
    DateTime? PaidOutAt,
    string ReceiverName,
    decimal Pot,
    string Status);
