using ClubMonitor.Domain.Fixtures;

namespace ClubMonitor.Application.Fixtures;

public sealed record RecordResultCommand(Guid FixtureId, int HomeScore, int AwayScore);

public sealed class RecordResultHandler(IFixtureRepository repo)
{
    public async Task<FixtureDto?> HandleAsync(RecordResultCommand command, CancellationToken ct = default)
    {
        var fixture = await repo.FindByIdAsync(FixtureId.From(command.FixtureId), ct);
        if (fixture is null) return null;

        fixture.RecordResult(command.HomeScore, command.AwayScore);
        await repo.SaveChangesAsync(ct);

        return CreateFixtureHandler.ToDto(fixture);
    }
}
