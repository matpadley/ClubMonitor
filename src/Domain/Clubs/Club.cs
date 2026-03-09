namespace ClubMonitor.Domain.Clubs;

public sealed class Club
{
    public ClubId Id { get; private set; }
    public string Name { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    private Club() { }

    private Club(ClubId id, string name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
    }

    public static Club Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > 200)
            throw new ArgumentException("Club name cannot exceed 200 characters.", nameof(name));
        return new Club(ClubId.New(), name.Trim(), DateTimeOffset.UtcNow);
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > 200)
            throw new ArgumentException("Club name cannot exceed 200 characters.", nameof(name));
        Name = name.Trim();
    }
}
