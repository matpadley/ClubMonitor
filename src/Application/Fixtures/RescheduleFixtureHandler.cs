using ClubMonitor.Domain.Fixtures;

namespace ClubMonitor.Application.Fixtures;

public sealed record RescheduleFixtureCommand(Guid FixtureId, DateTimeOffset ScheduledAt, string? Venue);

public sealed class RescheduleFixtureHandler(IFixtureRepository repo)
{
    public async Task<FixtureDto?> HandleAsync(RescheduleFixtureCommand command, CancellationToken ct = default)
    {
        var fixture = await repo.FindByIdAsync(FixtureId.From(command.FixtureId), ct);
        if (fixture is null) return null;

        fixture.Reschedule(command.ScheduledAt, command.Venue);
        await repo.SaveChangesAsync(ct);

        return CreateFixtureHandler.ToDto(fixture);
    }
}
