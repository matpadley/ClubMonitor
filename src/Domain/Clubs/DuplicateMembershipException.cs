namespace ClubMonitor.Domain.Clubs;

public sealed class DuplicateMembershipException(Guid clubId, Guid memberId)
    : Exception($"Member '{memberId}' is already a member of club '{clubId}'.");
