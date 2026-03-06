namespace ClubMonitor.Domain.Members;

public sealed class Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim().ToLowerInvariant();

        var atIndex = value.IndexOf('@');
        if (atIndex <= 0 || atIndex == value.Length - 1 || value.Length > 256)
            throw new ArgumentException("Invalid email address.", nameof(value));

        return new Email(value);
    }

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is Email other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
}
