using ClubMonitor.Domain.UserProfiles;

namespace ClubMonitor.Application.UserProfiles;

public sealed record RegisterUserCommand(string Username, string Email, string DisplayName, string? Bio);

public sealed record RegisterUserResult(
    Guid Id,
    string Username,
    string Email,
    string DisplayName,
    string? Bio,
    DateTimeOffset CreatedAt);

public sealed class RegisterUserHandler(IUserProfileRepository repository)
{
    public async Task<RegisterUserResult> HandleAsync(RegisterUserCommand command, CancellationToken ct = default)
    {
        var username = Username.Create(command.Username);

        if (await repository.ExistsWithUsernameAsync(username, ct))
            throw new DuplicateUsernameException(command.Username);

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        if (await repository.ExistsWithEmailAsync(normalizedEmail, ct))
            throw new DuplicateUserEmailException(command.Email);

        var profile = UserProfile.Create(command.Username, command.Email, command.DisplayName, command.Bio);

        await repository.AddAsync(profile, ct);
        await repository.SaveChangesAsync(ct);

        return new RegisterUserResult(
            profile.Id.Value,
            profile.Username.Value,
            profile.Email,
            profile.DisplayName,
            profile.Bio,
            profile.CreatedAt);
    }
}
