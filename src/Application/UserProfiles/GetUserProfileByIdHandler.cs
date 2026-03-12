using ClubMonitor.Domain.UserProfiles;

namespace ClubMonitor.Application.UserProfiles;

public sealed record GetUserProfileByIdQuery(Guid Id);

public sealed record UserProfileResult(
    Guid Id,
    string Username,
    string Email,
    string DisplayName,
    string? Bio,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed class GetUserProfileByIdHandler(IUserProfileRepository repository)
{
    public async Task<UserProfileResult?> HandleAsync(GetUserProfileByIdQuery query, CancellationToken ct = default)
    {
        var id = UserProfileId.From(query.Id);
        var profile = await repository.FindByIdAsync(id, ct);

        if (profile is null) return null;

        return new UserProfileResult(
            profile.Id.Value,
            profile.Username.Value,
            profile.Email,
            profile.DisplayName,
            profile.Bio,
            profile.CreatedAt,
            profile.UpdatedAt);
    }
}
