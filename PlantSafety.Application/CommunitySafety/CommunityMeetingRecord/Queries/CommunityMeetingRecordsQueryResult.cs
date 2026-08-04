// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetCommunityMeetingRecordsQuery.
// SchemaVersion: 1

using PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Models;

namespace PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Queries;

/// <summary>Result of <see cref="GetCommunityMeetingRecordsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching CommunityMeetingRecord items.</param>
public sealed record CommunityMeetingRecordsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<CommunityMeetingRecordItemDto> Items);
