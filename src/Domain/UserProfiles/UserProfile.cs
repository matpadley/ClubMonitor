namespace ClubMonitor.Domain.UserProfiles;

public sealed class UserProfile
{
    public UserProfileId Id { get; private set; }
    public Username Username { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string? Bio { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Required by EF Core
    private UserProfile() { }

    public static UserProfile Create(string username, string email, string displayName, string? bio = null)
    {
        return new UserProfile
        {
            Id = UserProfileId.New(),
            Username = Username.Create(username),
            Email = ValidateEmail(email),
            DisplayName = ValidateDisplayName(displayName),
            Bio = bio?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateProfile(string displayName, string? bio)
    {
        DisplayName = ValidateDisplayName(displayName);
        Bio = bio?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeEmail(string email)
    {
        Email = ValidateEmail(email);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeUsername(Username username)
    {
        ArgumentNullException.ThrowIfNull(username);
        Username = username;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string ValidateEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        email = email.Trim().ToLowerInvariant();
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1 || email.Length > 256)
            throw new ArgumentException("Invalid email address.", nameof(email));
        return email;
    }

    private static string ValidateDisplayName(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        if (displayName.Length > 200)
            throw new ArgumentException("Display name cannot exceed 200 characters.", nameof(displayName));
        return displayName.Trim();
    }
}
