namespace ClubMonitor.Domain.Cups;

public sealed class Cup
{
    public CupId Id { get; private set; }
    public string Name { get; private set; } = default!;
    public CupStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Cup() { }

    public static Cup Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > 200)
            throw new ArgumentException("Cup name cannot exceed 200 characters.", nameof(name));
        return new Cup
        {
            Id = CupId.New(),
            Name = name.Trim(),
            Status = CupStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > 200)
            throw new ArgumentException("Cup name cannot exceed 200 characters.", nameof(name));
        Name = name.Trim();
    }

    public void MarkDrawn()
    {
        Status = CupStatus.Drawn;
    }
}
