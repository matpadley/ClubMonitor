using ClubMonitor.Domain.Clubs;

namespace ClubMonitor.Domain.Leagues;

public interface ILeagueRepository
{
    Task<League?> FindByIdAsync(LeagueId id, CancellationToken ct = default);
    Task<IReadOnlyList<League>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(League league, CancellationToken ct = default);
    Task<bool> DeleteAsync(LeagueId id, CancellationToken ct = default);
    Task<bool> EntryExistsAsync(LeagueId leagueId, ClubId clubId, CancellationToken ct = default);
    Task<IReadOnlyList<LeagueEntry>> ListEntriesAsync(LeagueId leagueId, int skip, int take, CancellationToken ct = default);
    Task AddEntryAsync(LeagueEntry entry, CancellationToken ct = default);
    Task<bool> RemoveEntryAsync(LeagueId leagueId, ClubId clubId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
