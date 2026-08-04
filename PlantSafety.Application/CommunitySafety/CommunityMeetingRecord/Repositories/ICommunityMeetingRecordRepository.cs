// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the CommunityMeetingRecord read-model.
// SchemaVersion: 1

using PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Models;

namespace PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Repositories;

/// <summary>Read-only repository for CommunityMeetingRecord data. Implemented in the Infrastructure layer.</summary>
public interface ICommunityMeetingRecordRepository
{
    /// <summary>Returns CommunityMeetingRecord items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CommunityMeetingRecordItemDto>> GetCommunityMeetingRecordItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
