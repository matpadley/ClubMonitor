namespace ClubMonitor.Domain.UserProfiles;

public sealed class DuplicateUsernameException(string username)
    : Exception($"An account with username '{username}' already exists.");
