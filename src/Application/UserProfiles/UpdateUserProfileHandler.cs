using ClubMonitor.Domain.UserProfiles;

namespace ClubMonitor.Application.UserProfiles;

public sealed record UpdateUserProfileCommand(Guid Id, string DisplayName, string? Bio);

public sealed record UpdateUserProfileResult(
    Guid Id,
    string Username,
    string Email,
    string DisplayName,
    string? Bio,
    DateTimeOffset UpdatedAt);

public sealed class UpdateUserProfileHandler(IUserProfileRepository repository)
{
    public async Task<UpdateUserProfileResult?> HandleAsync(UpdateUserProfileCommand command, CancellationToken ct = default)
    {
        var id = UserProfileId.From(command.Id);
        var profile = await repository.FindByIdAsync(id, ct);

        if (profile is null) return null;

        profile.UpdateProfile(command.DisplayName, command.Bio);
        await repository.SaveChangesAsync(ct);

        return new UpdateUserProfileResult(
            profile.Id.Value,
            profile.Username.Value,
            profile.Email,
            profile.DisplayName,
            profile.Bio,
            profile.UpdatedAt);
    }
}
