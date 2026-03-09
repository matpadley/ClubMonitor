using ClubMonitor.Domain.Clubs;

namespace ClubMonitor.Domain.Cups;

public sealed class CupEntry
{
    public CupEntryId Id { get; private set; }
    public CupId CupId { get; private set; }
    public ClubId ClubId { get; private set; }
    public DateTimeOffset EnteredAt { get; private set; }

    private CupEntry() { }

    public static CupEntry Create(CupId cupId, ClubId clubId)
    {
        return new CupEntry
        {
            Id = CupEntryId.New(),
            CupId = cupId,
            ClubId = clubId,
            EnteredAt = DateTimeOffset.UtcNow
        };
    }
}
