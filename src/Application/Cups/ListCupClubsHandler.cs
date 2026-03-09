using ClubMonitor.Domain.Cups;

namespace ClubMonitor.Application.Cups;

public sealed record ListCupClubsQuery(Guid CupId, int Skip, int Take);

public sealed class ListCupClubsHandler(ICupRepository repo)
{
    public async Task<IReadOnlyList<CupEntryDto>> HandleAsync(ListCupClubsQuery query, CancellationToken ct = default)
    {
        var entries = await repo.ListEntriesAsync(CupId.From(query.CupId), query.Skip, query.Take, ct);
        return entries
            .Select(e => new CupEntryDto(e.Id.Value, e.CupId.Value, e.ClubId.Value, e.EnteredAt))
            .ToList();
    }
}
