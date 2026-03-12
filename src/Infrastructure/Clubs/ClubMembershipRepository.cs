using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Members;
using ClubMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClubMonitor.Infrastructure.Clubs;

internal sealed class ClubMembershipRepository(AppDbContext db) : IClubMembershipRepository
{
    public async Task<IReadOnlyList<ClubMembership>> ListByMemberAsync(MemberId memberId, int skip, int take, CancellationToken ct = default)
        => await db.ClubMemberships
            .Where(m => m.MemberId == memberId)
            .OrderBy(m => m.JoinedAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);
    public Task<bool> ExistsAsync(ClubId clubId, MemberId memberId, CancellationToken ct = default)
        => db.ClubMemberships.AnyAsync(m => m.ClubId == clubId && m.MemberId == memberId, ct);

    public async Task<IReadOnlyList<ClubMembership>> ListByClubAsync(ClubId clubId, int skip, int take, CancellationToken ct = default)
        => await db.ClubMemberships
            .Where(m => m.ClubId == clubId)
            .OrderBy(m => m.JoinedAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

    public async Task AddAsync(ClubMembership membership, CancellationToken ct = default)
        => await db.ClubMemberships.AddAsync(membership, ct);

    public async Task<bool> DeleteAsync(ClubId clubId, MemberId memberId, CancellationToken ct = default)
    {
        var membership = await db.ClubMemberships
            .FirstOrDefaultAsync(m => m.ClubId == clubId && m.MemberId == memberId, ct);
        if (membership is null) return false;
        db.ClubMemberships.Remove(membership);
        return true;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
