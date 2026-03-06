using ClubMonitor.Domain.Members;
using ClubMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClubMonitor.Infrastructure.Members;

internal sealed class MemberRepository(AppDbContext db) : IMemberRepository
{
    public Task<Member?> FindByIdAsync(MemberId id, CancellationToken ct = default)
        => db.Members.FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<bool> ExistsWithEmailAsync(Email email, CancellationToken ct = default)
        => db.Members.AnyAsync(m => m.Email == email, ct);

    public async Task AddAsync(Member member, CancellationToken ct = default)
        => await db.Members.AddAsync(member, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
