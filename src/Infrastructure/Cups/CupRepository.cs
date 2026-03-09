using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Cups;
using ClubMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClubMonitor.Infrastructure.Cups;

internal sealed class CupRepository(AppDbContext db) : ICupRepository
{
    public Task<Cup?> FindByIdAsync(CupId id, CancellationToken ct = default)
        => db.Cups.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Cup>> ListAsync(int skip, int take, CancellationToken ct = default)
        => await db.Cups.OrderBy(c => c.Name).Skip(skip).Take(take).ToListAsync(ct);

    public Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default)
        => db.Cups.AnyAsync(c => c.Name == name, ct);

    public async Task AddAsync(Cup cup, CancellationToken ct = default)
        => await db.Cups.AddAsync(cup, ct);

    public async Task<bool> DeleteAsync(CupId id, CancellationToken ct = default)
    {
        var cup = await db.Cups.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (cup is null) return false;
        db.Cups.Remove(cup);
        return true;
    }

    public Task<bool> EntryExistsAsync(CupId cupId, ClubId clubId, CancellationToken ct = default)
        => db.CupEntries.AnyAsync(e => e.CupId == cupId && e.ClubId == clubId, ct);

    public async Task<IReadOnlyList<CupEntry>> ListEntriesAsync(CupId cupId, int skip, int take, CancellationToken ct = default)
        => await db.CupEntries
            .Where(e => e.CupId == cupId)
            .OrderBy(e => e.EnteredAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

    public async Task AddEntryAsync(CupEntry entry, CancellationToken ct = default)
        => await db.CupEntries.AddAsync(entry, ct);

    public async Task<bool> RemoveEntryAsync(CupId cupId, ClubId clubId, CancellationToken ct = default)
    {
        var entry = await db.CupEntries
            .FirstOrDefaultAsync(e => e.CupId == cupId && e.ClubId == clubId, ct);
        if (entry is null) return false;
        db.CupEntries.Remove(entry);
        return true;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
