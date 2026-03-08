using ClubMonitor.Domain.Cups;

namespace ClubMonitor.Application.Cups;

public sealed record GetCupByIdQuery(Guid Id);

public sealed class GetCupByIdHandler(ICupRepository repo)
{
    public async Task<CupDto?> HandleAsync(GetCupByIdQuery query, CancellationToken ct = default)
    {
        var cup = await repo.FindByIdAsync(CupId.From(query.Id), ct);
        if (cup is null) return null;
        return new CupDto(cup.Id.Value, cup.Name, cup.Status, cup.CreatedAt);
    }
}
