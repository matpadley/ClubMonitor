using ClubMonitor.Domain.Clubs;

namespace ClubMonitor.Application.Clubs;

public sealed record DeleteClubCommand(Guid Id);

public sealed class DeleteClubHandler(IClubRepository repo)
{
    public Task<bool> HandleAsync(DeleteClubCommand command, CancellationToken ct = default)
        => repo.DeleteAsync(ClubId.From(command.Id), ct);
}
