namespace ClubMonitor.Domain.UserProfiles;

public interface IUserProfileRepository
{
    Task<UserProfile?> FindByIdAsync(UserProfileId id, CancellationToken ct = default);
    Task<bool> ExistsWithUsernameAsync(Username username, CancellationToken ct = default);
    Task<bool> ExistsWithUsernameAsync(Username username, UserProfileId excludingId, CancellationToken ct = default);
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsWithEmailAsync(string email, UserProfileId excludingId, CancellationToken ct = default);
    Task AddAsync(UserProfile profile, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
