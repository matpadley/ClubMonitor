using ClubMonitor.Domain.Members;

namespace ClubMonitor.Application.Members;

public sealed record GetMemberByIdQuery(Guid Id);

public sealed record MemberDto(Guid Id, string Name, string Email, DateTimeOffset CreatedAt);

public sealed class GetMemberByIdHandler(IMemberRepository repo)
{
    public async Task<MemberDto?> HandleAsync(GetMemberByIdQuery query, CancellationToken ct = default)
    {
        var member = await repo.FindByIdAsync(MemberId.From(query.Id), ct);
        if (member is null) return null;

        return new MemberDto(member.Id.Value, member.Name, member.Email.Value, member.CreatedAt);
    }
}
