using ClubMonitor.Domain.Members;
using ClubMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClubMonitor.Infrastructure.Members;

internal sealed class MemberRepository(AppDbContext db) : IMemberRepository
{
    public Task<Member?> FindByIdAsync(MemberId id, CancellationToken ct = default)
        => db.Members.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<Member>> ListAsync(int skip, int take, CancellationToken ct = default)
        => await db.Members.OrderByDescending(m => m.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);

    public Task<bool> ExistsWithEmailAsync(Email email, CancellationToken ct = default)
        => db.Members.AnyAsync(m => m.Email == email, ct);

    public Task<bool> ExistsWithEmailAsync(Email email, MemberId excludingMemberId, CancellationToken ct = default)
        => db.Members.AnyAsync(m => m.Email == email && m.Id != excludingMemberId, ct);

    public async Task AddAsync(Member member, CancellationToken ct = default)
        => await db.Members.AddAsync(member, ct);

    public async Task<bool> DeleteAsync(MemberId id, CancellationToken ct = default)
    {
        var member = await db.Members.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (member is null) return false;
        db.Members.Remove(member);
        return true;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
