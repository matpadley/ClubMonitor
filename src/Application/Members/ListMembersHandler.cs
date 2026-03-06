using ClubMonitor.Domain.Members;

namespace ClubMonitor.Application.Members;

public sealed record ListMembersQuery(int Skip = 0, int Take = 50);

public sealed class ListMembersHandler(IMemberRepository repo)
{
    public async Task<IReadOnlyList<MemberDto>> HandleAsync(ListMembersQuery query, CancellationToken ct = default)
    {
        var members = await repo.ListAsync(query.Skip, query.Take, ct);
        return members.Select(m => new MemberDto(m.Id.Value, m.Name, m.Email.Value, m.CreatedAt)).ToList();
    }
}
