using ClubMonitor.Domain.Clubs;

namespace ClubMonitor.Application.Clubs;

public sealed record CreateClubCommand(string Name);

public sealed record ClubDto(Guid Id, string Name, DateTimeOffset CreatedAt);

public sealed class CreateClubHandler(IClubRepository repo)
{
    public async Task<ClubDto> HandleAsync(CreateClubCommand command, CancellationToken ct = default)
    {
        if (await repo.ExistsWithNameAsync(command.Name, ct))
            throw new DuplicateClubNameException(command.Name);

        var club = Club.Create(command.Name);
        await repo.AddAsync(club, ct);
        await repo.SaveChangesAsync(ct);

        return new ClubDto(club.Id.Value, club.Name, club.CreatedAt);
    }
}
