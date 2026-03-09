using ClubMonitor.Domain.Fixtures;
using ClubMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClubMonitor.Infrastructure.Fixtures;

internal sealed class FixtureRepository(AppDbContext db) : IFixtureRepository
{
    public Task<Fixture?> FindByIdAsync(FixtureId id, CancellationToken ct = default)
        => db.Fixtures.FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<IReadOnlyList<Fixture>> ListByCompetitionAsync(
        CompetitionType type, Guid competitionId, int skip, int take, CancellationToken ct = default)
        => await db.Fixtures
            .Where(f => f.CompetitionType == type && f.CompetitionId == competitionId)
            .OrderBy(f => f.ScheduledAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Fixture>> ListPlayedByCompetitionAsync(
        CompetitionType type, Guid competitionId, CancellationToken ct = default)
        => await db.Fixtures
            .Where(f => f.CompetitionType == type && f.CompetitionId == competitionId && f.Status == FixtureStatus.Played)
            .ToListAsync(ct);

    public async Task AddAsync(Fixture fixture, CancellationToken ct = default)
        => await db.Fixtures.AddAsync(fixture, ct);

    public async Task AddRangeAsync(IEnumerable<Fixture> fixtures, CancellationToken ct = default)
        => await db.Fixtures.AddRangeAsync(fixtures, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
