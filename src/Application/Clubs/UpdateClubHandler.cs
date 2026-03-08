using ClubMonitor.Domain.Clubs;

namespace ClubMonitor.Application.Clubs;

public sealed record UpdateClubCommand(Guid Id, string Name);

public sealed class UpdateClubHandler(IClubRepository repo)
{
    public async Task<ClubDto?> HandleAsync(UpdateClubCommand command, CancellationToken ct = default)
    {
        var club = await repo.FindByIdAsync(ClubId.From(command.Id), ct);
        if (club is null) return null;

        var newName = command.Name.Trim();
        if (!string.Equals(club.Name, newName, StringComparison.OrdinalIgnoreCase))
        {
            if (await repo.ExistsWithNameAsync(newName, ct))
                throw new DuplicateClubNameException(newName);
        }

        club.Rename(newName);
        await repo.SaveChangesAsync(ct);

        return new ClubDto(club.Id.Value, club.Name, club.CreatedAt);
    }
}
