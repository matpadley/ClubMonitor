using ClubMonitor.Domain.Clubs;

namespace ClubMonitor.Application.Clubs;

public sealed record GetClubByIdQuery(Guid Id);

public sealed class GetClubByIdHandler(IClubRepository repo)
{
    public async Task<ClubDto?> HandleAsync(GetClubByIdQuery query, CancellationToken ct = default)
    {
        var club = await repo.FindByIdAsync(ClubId.From(query.Id), ct);
        if (club is null) return null;
        return new ClubDto(club.Id.Value, club.Name, club.CreatedAt);
    }
}
