namespace ClubMonitor.Domain.Leagues;

public sealed class DuplicateLeagueEntryException(Guid leagueId, Guid clubId)
    : Exception($"Club '{clubId}' is already entered in league '{leagueId}'.");
