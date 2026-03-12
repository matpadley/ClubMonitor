using ClubMonitor.Domain.UserProfiles;
using ClubMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClubMonitor.Infrastructure.UserProfiles;

internal sealed class UserProfileRepository(AppDbContext db) : IUserProfileRepository
{
    public Task<UserProfile?> FindByIdAsync(UserProfileId id, CancellationToken ct = default)
        => db.UserProfiles.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<bool> ExistsWithUsernameAsync(Username username, CancellationToken ct = default)
        => db.UserProfiles.AnyAsync(u => u.Username == username, ct);

    public Task<bool> ExistsWithUsernameAsync(Username username, UserProfileId excludingId, CancellationToken ct = default)
        => db.UserProfiles.AnyAsync(u => u.Username == username && u.Id != excludingId, ct);

    public Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default)
        => db.UserProfiles.AnyAsync(u => u.Email == email, ct);

    public Task<bool> ExistsWithEmailAsync(string email, UserProfileId excludingId, CancellationToken ct = default)
        => db.UserProfiles.AnyAsync(u => u.Email == email && u.Id != excludingId, ct);

    public async Task AddAsync(UserProfile profile, CancellationToken ct = default)
        => await db.UserProfiles.AddAsync(profile, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
