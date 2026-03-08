using ClubMonitor.Domain.Cups;

namespace ClubMonitor.Application.Cups;

public sealed record ListCupsQuery(int Skip, int Take);

public sealed class ListCupsHandler(ICupRepository repo)
{
    public async Task<IReadOnlyList<CupDto>> HandleAsync(ListCupsQuery query, CancellationToken ct = default)
    {
        var cups = await repo.ListAsync(query.Skip, query.Take, ct);
        return cups.Select(c => new CupDto(c.Id.Value, c.Name, c.Status, c.CreatedAt)).ToList();
    }
}
