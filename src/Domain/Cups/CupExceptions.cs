namespace ClubMonitor.Domain.Cups;

public sealed class DuplicateCupEntryException(Guid cupId, Guid clubId)
    : Exception($"Club '{clubId}' is already entered in cup '{cupId}'.");

public sealed class InvalidCupStateException(string message) : Exception(message);
