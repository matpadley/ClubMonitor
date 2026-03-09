using ClubMonitor.Domain.Clubs;

namespace ClubMonitor.Domain.Cups;

public interface ICupRepository
{
    Task<Cup?> FindByIdAsync(CupId id, CancellationToken ct = default);
    Task<IReadOnlyList<Cup>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Cup cup, CancellationToken ct = default);
    Task<bool> DeleteAsync(CupId id, CancellationToken ct = default);
    Task<bool> EntryExistsAsync(CupId cupId, ClubId clubId, CancellationToken ct = default);
    Task<IReadOnlyList<CupEntry>> ListEntriesAsync(CupId cupId, int skip, int take, CancellationToken ct = default);
    Task AddEntryAsync(CupEntry entry, CancellationToken ct = default);
    Task<bool> RemoveEntryAsync(CupId cupId, ClubId clubId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
