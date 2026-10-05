using EkubCircle.Application.Interfaces;
using EkubCircle.Application.DTOs;
using EkubCircle.Domain.Enums;
using EkubCircle.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.Application.Services;

public class RoundService(IApplicationDbContext db) : IRoundService
{
    public async Task<RoundDto> GetCurrentAsync(int userId, int circleId)
    {
        await EnsureMemberAsync(userId, circleId);
        var round = await GetCurrentRoundEntityAsync(circleId)
                    ?? throw new KeyNotFoundException("There is no open round. The circle may be completed.");
        return await ToDtoAsync(round);
    }

    public async Task<IReadOnlyList<RoundDto>> GetRoundsAsync(int userId, int circleId)
    {
        await EnsureMemberAsync(userId, circleId);
        var rounds = await db.Rounds
            .Include(r => r.ReceiverMembership).ThenInclude(m => m.User)
            .Include(r => r.Payments)
            .Include(r => r.Circle).ThenInclude(c => c.Memberships)
            .Where(r => r.CircleId == circleId)
            .OrderBy(r => r.RoundNumber)
            .ToListAsync();
        var receivedIds = (await db.Payouts
            .Where(p => p.Round.CircleId == circleId && p.Status == PayoutStatus.Completed)
            .Select(p => p.ReceiverMembershipId).ToListAsync()).ToHashSet();
        return rounds.Select(r => ToDto(r, receivedIds.Contains(r.ReceiverMembershipId))).ToList();
    }

    public async Task<IReadOnlyList<HistoryItemDto>> GetHistoryAsync(int userId, int circleId)
    {
        await EnsureMemberAsync(userId, circleId);
        var rounds = await db.Rounds
            .Include(r => r.ReceiverMembership).ThenInclude(m => m.User)
            .Include(r => r.Payments)
            .Include(r => r.Circle).ThenInclude(c => c.Memberships)
            .Where(r => r.CircleId == circleId)
            .OrderBy(r => r.RoundNumber)
            .ToListAsync();

        return rounds.Select(r => new HistoryItemDto(
            r.RoundNumber,
            r.PaidOutAt,
            r.ReceiverMembership.User.FullName,
            r.Payments.Sum(p => p.Amount),
            r.Status.ToString())).ToList();
    }

    public async Task<PaymentResponse> MarkPaymentAsync(int userId, int roundId, int membershipId, MarkPaymentRequest request)
    {
        var round = await db.Rounds.Include(r => r.Circle).FirstOrDefaultAsync(r => r.Id == roundId)
                    ?? throw new KeyNotFoundException("Round not found.");
        if (round.Circle.OrganizerId != userId)
        {
            var actorMembership = await db.CircleMemberships.FirstOrDefaultAsync(m =>
                m.Id == membershipId && m.CircleId == round.CircleId && m.UserId == userId && m.MembershipStatus == MembershipStatus.Active);
            if (actorMembership is null)
                throw new UnauthorizedAccessException("You can only simulate payment for your own membership.");
        }
        if (round.Status != RoundStatus.Open)
            throw new InvalidOperationException("Payments cannot be recorded after payout.");

        var membership = await db.CircleMemberships.FirstOrDefaultAsync(m =>
            m.Id == membershipId && m.CircleId == round.CircleId && m.MembershipStatus == MembershipStatus.Active)
            ?? throw new KeyNotFoundException("Active circle membership not found.");

        if (await db.Payments.AnyAsync(p => p.RoundId == roundId && p.MembershipId == membershipId))
            throw new InvalidOperationException("This member has already been marked paid for this round.");

        var amount = request.Amount ?? round.Circle.ContributionAmount;
        if (amount != round.Circle.ContributionAmount)
            throw new InvalidOperationException("Payment amount must equal the circle contribution.");

        db.Payments.Add(new Payment
        {
            RoundId = roundId,
            MembershipId = membership.Id,
            Amount = amount,
            PaidAt = request.PaidAt?.ToUniversalTime() ?? DateTime.UtcNow,
            ReferenceNote = request.ReferenceNote?.Trim(),
            RecordedByUserId = userId
        });
        await db.SaveChangesAsync();
        return new PaymentResponse(round.Id, round.CircleId, membership.Id, amount, "Payment recorded.");
    }

    public async Task<PayoutResponse> PayoutAsync(int organizerId, int circleId)
    {
        var circle = await db.Circles.Include(c => c.Memberships)
            .FirstOrDefaultAsync(c => c.Id == circleId)
            ?? throw new KeyNotFoundException("Circle not found.");
        if (circle.OrganizerId != organizerId)
            throw new UnauthorizedAccessException("Only the organizer can pay out the round.");
        if (circle.Status != CircleStatus.Active)
            throw new InvalidOperationException("Only an active circle can pay out.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        var round = await db.Rounds
            .Include(r => r.ReceiverMembership).ThenInclude(m => m.User)
            .Include(r => r.Payments)
            .Include(r => r.Circle).ThenInclude(c => c.Memberships)
            .Where(r => r.CircleId == circleId && r.Status == RoundStatus.Open)
            .OrderBy(r => r.RoundNumber)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("There is no open round to pay out.");

        var activeMembershipIds = circle.Memberships
            .Where(m => m.MembershipStatus == MembershipStatus.Active)
            .Select(m => m.Id).ToList();

        var paidIds = round.Payments.Select(p => p.MembershipId).ToHashSet();
        if (activeMembershipIds.Any(id => !paidIds.Contains(id)))
            throw new InvalidOperationException("Cannot pay out. All active members must be paid for this round.");

        var alreadyReceived = await db.Payouts.AnyAsync(p =>
            p.ReceiverMembershipId == round.ReceiverMembershipId && p.Status == PayoutStatus.Completed);
        if (alreadyReceived)
            throw new InvalidOperationException("The receiver has already received a successful payout in this circle.");

        if (await db.Payouts.AnyAsync(p => p.RoundId == round.Id && p.Status == PayoutStatus.Completed))
            throw new InvalidOperationException("This round has already been paid out.");

        var amount = round.Payments.Sum(p => p.Amount);
        db.Payouts.Add(new Payout
        {
            RoundId = round.Id,
            ReceiverMembershipId = round.ReceiverMembershipId,
            Amount = amount,
            ConfirmedByUserId = organizerId,
            Status = PayoutStatus.Completed
        });
        round.PayoutAmount = amount;
        round.PaidOutAt = DateTime.UtcNow;
        round.Status = RoundStatus.PaidOut;

        var allReceived = true;
        foreach (var membershipId in activeMembershipIds)
        {
            var received = await db.Payouts.AnyAsync(p =>
                p.ReceiverMembershipId == membershipId && p.Status == PayoutStatus.Completed);
            if (membershipId == round.ReceiverMembershipId) received = true;
            if (!received) { allReceived = false; break; }
        }
        if (allReceived) circle.Status = CircleStatus.Completed;

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new PayoutResponse(round.Id, round.RoundNumber, round.ReceiverMembership.User.FullName,
            amount, round.Status.ToString(),
            circle.Status == CircleStatus.Completed ? "Circle completed. Every member has received once." : "Payout completed. Next round is now current.");
    }

    private async Task<Round?> GetCurrentRoundEntityAsync(int circleId) => await db.Rounds
        .Include(r => r.ReceiverMembership).ThenInclude(m => m.User)
        .Include(r => r.Payments)
        .Include(r => r.Circle).ThenInclude(c => c.Memberships)
        .Where(r => r.CircleId == circleId && r.Status == RoundStatus.Open)
        .OrderBy(r => r.RoundNumber)
        .FirstOrDefaultAsync();

    private async Task<RoundDto> ToDtoAsync(Round r)
    {
        var receiverHasReceived = await db.Payouts.AnyAsync(p =>
            p.ReceiverMembershipId == r.ReceiverMembershipId && p.Status == PayoutStatus.Completed);
        return ToDto(r, receiverHasReceived);
    }

    private static RoundDto ToDto(Round r, bool receiverHasReceived)
    {
        return new RoundDto(
            r.Id,
            r.RoundNumber,
            r.Status.ToString(),
            r.Circle.ContributionAmount,
            r.Payments.Sum(p => p.Amount),
            r.Payments.Count,
            r.Circle.Memberships.Count(m => m.MembershipStatus == MembershipStatus.Active),
            r.ReceiverMembershipId,
            r.ReceiverMembership.User.FullName,
            receiverHasReceived,
            r.OpenedAt,
            r.PaidOutAt,
            r.PayoutAmount);
    }

    private async Task EnsureMemberAsync(int userId, int circleId)
    {
        var exists = await db.CircleMemberships.AnyAsync(m =>
            m.UserId == userId && m.CircleId == circleId && m.MembershipStatus == MembershipStatus.Active);
        if (!exists) throw new UnauthorizedAccessException("You are not an active member of this circle.");
    }
}
