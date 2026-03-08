namespace ClubMonitor.Domain.Leagues;

public sealed class DuplicateLeagueNameException(string name)
    : Exception($"A league with name '{name}' already exists.");
