using ClubMonitor.Domain.Members;

namespace ClubMonitor.Application.Members;

public sealed record CreateMemberCommand(string Name, string Email);

public sealed class CreateMemberHandler(IMemberRepository repo)
{
    public async Task<MemberDto> HandleAsync(CreateMemberCommand command, CancellationToken ct = default)
    {
        var email = Email.Create(command.Email);

        if (await repo.ExistsWithEmailAsync(email, ct))
            throw new DuplicateEmailException(command.Email);

        var member = Member.Create(command.Name, email);
        await repo.AddAsync(member, ct);
        await repo.SaveChangesAsync(ct);

        return new MemberDto(member.Id.Value, member.Name, member.Email.Value, member.CreatedAt);
    }
}
