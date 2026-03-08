using ClubMonitor.Domain.Members;

namespace ClubMonitor.Domain.Clubs;

public interface IClubMembershipRepository
{
    Task<bool> ExistsAsync(ClubId clubId, MemberId memberId, CancellationToken ct = default);
    Task<IReadOnlyList<ClubMembership>> ListByClubAsync(ClubId clubId, int skip, int take, CancellationToken ct = default);
    Task AddAsync(ClubMembership membership, CancellationToken ct = default);
    Task<bool> DeleteAsync(ClubId clubId, MemberId memberId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
