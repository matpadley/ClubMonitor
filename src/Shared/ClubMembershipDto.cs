namespace ClubMonitor.Shared;

public sealed record ClubMembershipDto(
    Guid Id,
    Guid ClubId,
    Guid MemberId,
    string Role,
    DateTimeOffset JoinedAt);
