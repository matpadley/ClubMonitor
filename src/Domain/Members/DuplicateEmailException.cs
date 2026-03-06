namespace ClubMonitor.Domain.Members;

public sealed class DuplicateEmailException(string email)
    : Exception($"A member with email '{email}' already exists.");
