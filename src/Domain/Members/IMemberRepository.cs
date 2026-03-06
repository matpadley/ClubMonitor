namespace ClubMonitor.Domain.Members;

public interface IMemberRepository
{
    Task<Member?> FindByIdAsync(MemberId id, CancellationToken ct = default);
    Task<IReadOnlyList<Member>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<bool> ExistsWithEmailAsync(Email email, CancellationToken ct = default);
    Task<bool> ExistsWithEmailAsync(Email email, MemberId excludingMemberId, CancellationToken ct = default);
    Task AddAsync(Member member, CancellationToken ct = default);
    Task<bool> DeleteAsync(MemberId id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
