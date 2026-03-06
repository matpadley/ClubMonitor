using ClubMonitor.Domain.Members;

namespace ClubMonitor.Application.Members;

public sealed record UpdateMemberCommand(Guid Id, string Name, string Email);

public sealed class UpdateMemberHandler(IMemberRepository repo)
{
    public async Task<MemberDto?> HandleAsync(UpdateMemberCommand command, CancellationToken ct = default)
    {
        var member = await repo.FindByIdAsync(MemberId.From(command.Id), ct);
        if (member is null) return null;

        var email = Email.Create(command.Email);

        if (await repo.ExistsWithEmailAsync(email, MemberId.From(command.Id), ct))
            throw new DuplicateEmailException(command.Email);

        member.Rename(command.Name);
        member.ChangeEmail(email);
        await repo.SaveChangesAsync(ct);

        return new MemberDto(member.Id.Value, member.Name, member.Email.Value, member.CreatedAt);
    }
}
