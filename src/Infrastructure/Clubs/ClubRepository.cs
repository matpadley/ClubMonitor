using ClubMonitor.Domain.Clubs;
using ClubMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClubMonitor.Infrastructure.Clubs;

internal sealed class ClubRepository(AppDbContext db) : IClubRepository
{
    public Task<Club?> FindByIdAsync(ClubId id, CancellationToken ct = default)
        => db.Clubs.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Club>> ListAsync(int skip, int take, CancellationToken ct = default)
        => await db.Clubs.OrderBy(c => c.Name).Skip(skip).Take(take).ToListAsync(ct);

    public Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default)
        => db.Clubs.AnyAsync(c => c.Name == name, ct);

    public async Task AddAsync(Club club, CancellationToken ct = default)
        => await db.Clubs.AddAsync(club, ct);

    public async Task<bool> DeleteAsync(ClubId id, CancellationToken ct = default)
    {
        var club = await db.Clubs.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (club is null) return false;
        db.Clubs.Remove(club);
        return true;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
