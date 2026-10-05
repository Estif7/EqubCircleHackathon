using EkubCircle.Application.DTOs;

namespace EkubCircle.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(int userId);
}
