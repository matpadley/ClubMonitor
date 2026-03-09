namespace ClubMonitor.Domain.Fixtures;

public interface IFixtureRepository
{
    Task<Fixture?> FindByIdAsync(FixtureId id, CancellationToken ct = default);
    Task<IReadOnlyList<Fixture>> ListByCompetitionAsync(CompetitionType type, Guid competitionId, int skip, int take, CancellationToken ct = default);
    Task<IReadOnlyList<Fixture>> ListPlayedByCompetitionAsync(CompetitionType type, Guid competitionId, CancellationToken ct = default);
    Task AddAsync(Fixture fixture, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Fixture> fixtures, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
