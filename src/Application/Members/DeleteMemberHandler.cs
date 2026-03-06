using ClubMonitor.Domain.Members;

namespace ClubMonitor.Application.Members;

public sealed record DeleteMemberCommand(Guid Id);

public sealed class DeleteMemberHandler(IMemberRepository repo)
{
    public Task<bool> HandleAsync(DeleteMemberCommand command, CancellationToken ct = default)
        => repo.DeleteAsync(MemberId.From(command.Id), ct);
}
