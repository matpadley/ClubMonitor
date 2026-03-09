using ClubMonitor.Domain.Fixtures;

namespace ClubMonitor.Application.Fixtures;

public sealed record ListFixturesByCompetitionQuery(CompetitionType Type, Guid CompetitionId, int Skip, int Take);

public sealed class ListFixturesByCompetitionHandler(IFixtureRepository repo)
{
    public async Task<IReadOnlyList<FixtureDto>> HandleAsync(ListFixturesByCompetitionQuery query, CancellationToken ct = default)
    {
        var fixtures = await repo.ListByCompetitionAsync(query.Type, query.CompetitionId, query.Skip, query.Take, ct);
        return fixtures.Select(CreateFixtureHandler.ToDto).ToList();
    }
}
