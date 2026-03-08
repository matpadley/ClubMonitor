namespace ClubMonitor.Domain.Clubs;

public interface IClubRepository
{
    Task<Club?> FindByIdAsync(ClubId id, CancellationToken ct = default);
    Task<IReadOnlyList<Club>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Club club, CancellationToken ct = default);
    Task<bool> DeleteAsync(ClubId id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
