namespace ClubMonitor.Domain.UserProfiles;

public sealed class Username
{
    public string Value { get; }

    private Username(string value) => Value = value;

    public static Username Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        if (value.Length < 3 || value.Length > 50)
            throw new ArgumentException("Username must be between 3 and 50 characters.", nameof(value));

        foreach (var c in value)
        {
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                throw new ArgumentException("Username may only contain letters, digits, hyphens, and underscores.", nameof(value));
        }

        return new Username(value);
    }

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is Username other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
}
