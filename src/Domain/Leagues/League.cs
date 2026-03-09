namespace ClubMonitor.Domain.Leagues;

public sealed class League
{
    public LeagueId Id { get; private set; }
    public string Name { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    private League() { }

    public static League Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > 200)
            throw new ArgumentException("League name cannot exceed 200 characters.", nameof(name));
        return new League
        {
            Id = LeagueId.New(),
            Name = name.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > 200)
            throw new ArgumentException("League name cannot exceed 200 characters.", nameof(name));
        Name = name.Trim();
    }
}
