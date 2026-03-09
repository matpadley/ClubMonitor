namespace ClubMonitor.Domain.Clubs;

public sealed class DuplicateClubNameException(string name)
    : Exception($"A club with name '{name}' already exists.");
