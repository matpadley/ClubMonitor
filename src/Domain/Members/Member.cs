namespace ClubMonitor.Domain.Members;

public sealed class Member
{
    public MemberId Id { get; private set; }
    public string Name { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    // Required by EF Core
    private Member() { }

    private Member(MemberId id, string name, Email email, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Email = email;
        CreatedAt = createdAt;
    }

    public static Member Create(string name, Email email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Member(MemberId.New(), name, email, DateTimeOffset.UtcNow);
    }
}
