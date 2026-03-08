using ClubMonitor.Domain.Fixtures;

namespace ClubMonitor.Application.Fixtures;

public sealed record GetFixtureByIdQuery(Guid Id);

public sealed class GetFixtureByIdHandler(IFixtureRepository repo)
{
    public async Task<FixtureDto?> HandleAsync(GetFixtureByIdQuery query, CancellationToken ct = default)
    {
        var fixture = await repo.FindByIdAsync(FixtureId.From(query.Id), ct);
        if (fixture is null) return null;
        return CreateFixtureHandler.ToDto(fixture);
    }
}
