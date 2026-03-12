namespace ClubMonitor.Domain.UserProfiles;

public sealed class DuplicateUserEmailException(string email)
    : Exception($"An account with email '{email}' already exists.");
