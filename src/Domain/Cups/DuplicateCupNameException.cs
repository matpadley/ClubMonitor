namespace ClubMonitor.Domain.Cups;

public sealed class DuplicateCupNameException(string name)
    : Exception($"A cup with name '{name}' already exists.");
