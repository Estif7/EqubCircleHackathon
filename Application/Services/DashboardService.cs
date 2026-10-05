using EkubCircle.Application.DTOs;
using EkubCircle.Application.Interfaces;
using EkubCircle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.Application.Services;

public class DashboardService(IApplicationDbContext db) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(int userId)
    {
        var memberships = await db.CircleMemberships
            .Include(m => m.Circle)
            .ThenInclude(c => c.Memberships)
            .Include(m => m.Circle)
            .ThenInclude(c => c.Organizer)
            .Where(m => m.UserId == userId && m.MembershipStatus == MembershipStatus.Active && m.Circle.Status == CircleStatus.Active)
            .OrderBy(m => m.Circle.CreatedAt)
            .ToListAsync();

        DashboardNextCircleDto? nextPayment = null;
        DashboardNextCircleDto? nextPayout = null;

        foreach (var membership in memberships)
        {
            var circle = membership.Circle;
            var round = await db.Rounds
                .AsNoTracking()
                .Include(r => r.ReceiverMembership).ThenInclude(m => m.User)
                .Include(r => r.Payments)
                .Where(r => r.CircleId == circle.Id && r.Status == RoundStatus.Open)
                .OrderBy(r => r.RoundNumber)
                .FirstOrDefaultAsync();

            if (round is null) continue;

            var paidByUser = round.Payments.Any(p => p.MembershipId == membership.Id);
            var paidCount = round.Payments.Count;
            var totalMembers = circle.Memberships.Count(m => m.MembershipStatus == MembershipStatus.Active);
            var ready = paidCount == totalMembers;
            var receiverName = round.ReceiverMembership.User.FullName;

            if (!paidByUser && nextPayment is null)
            {
                nextPayment = new DashboardNextCircleDto(
                    circle.Id, circle.Name, circle.ContributionAmount, circle.Frequency.ToString(),
                    round.RoundNumber, receiverName, ready, "PaymentDue",
                    $"Round {round.RoundNumber}: contribute {circle.ContributionAmount:0.##} ETB.");
            }

            if (round.ReceiverMembershipId == membership.Id && nextPayout is null)
            {
                nextPayout = new DashboardNextCircleDto(
                    circle.Id, circle.Name, circle.ContributionAmount, circle.Frequency.ToString(),
                    round.RoundNumber, receiverName, ready, ready ? "Ready" : "WaitingForPayments",
                    ready ? "All members have paid. Your payout is ready." : $"Waiting for {totalMembers - paidCount} member payment(s).");
            }
        }

        return new DashboardSummaryDto(nextPayment, nextPayout);
    }
}
