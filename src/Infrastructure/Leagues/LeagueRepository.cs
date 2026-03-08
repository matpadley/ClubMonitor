using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Leagues;
using ClubMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClubMonitor.Infrastructure.Leagues;

internal sealed class LeagueRepository(AppDbContext db) : ILeagueRepository
{
    public Task<League?> FindByIdAsync(LeagueId id, CancellationToken ct = default)
        => db.Leagues.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<IReadOnlyList<League>> ListAsync(int skip, int take, CancellationToken ct = default)
        => await db.Leagues.OrderBy(l => l.Name).Skip(skip).Take(take).ToListAsync(ct);

    public Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default)
        => db.Leagues.AnyAsync(l => l.Name == name, ct);

    public async Task AddAsync(League league, CancellationToken ct = default)
        => await db.Leagues.AddAsync(league, ct);

    public async Task<bool> DeleteAsync(LeagueId id, CancellationToken ct = default)
    {
        var league = await db.Leagues.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (league is null) return false;
        db.Leagues.Remove(league);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public Task<bool> EntryExistsAsync(LeagueId leagueId, ClubId clubId, CancellationToken ct = default)
        => db.LeagueEntries.AnyAsync(e => e.LeagueId == leagueId && e.ClubId == clubId, ct);

    public async Task<IReadOnlyList<LeagueEntry>> ListEntriesAsync(LeagueId leagueId, int skip, int take, CancellationToken ct = default)
        => await db.LeagueEntries
            .Where(e => e.LeagueId == leagueId)
            .OrderBy(e => e.EnteredAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

    public async Task AddEntryAsync(LeagueEntry entry, CancellationToken ct = default)
        => await db.LeagueEntries.AddAsync(entry, ct);

    public async Task<bool> RemoveEntryAsync(LeagueId leagueId, ClubId clubId, CancellationToken ct = default)
    {
        var entry = await db.LeagueEntries
            .FirstOrDefaultAsync(e => e.LeagueId == leagueId && e.ClubId == clubId, ct);
        if (entry is null) return false;
        db.LeagueEntries.Remove(entry);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
