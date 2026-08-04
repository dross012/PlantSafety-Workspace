// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the CommunityContact read-model.
// SchemaVersion: 1

using PlantSafety.Application.CommunitySafety.CommunityContact.Models;

namespace PlantSafety.Application.CommunitySafety.CommunityContact.Repositories;

/// <summary>Read-only repository for CommunityContact data. Implemented in the Infrastructure layer.</summary>
public interface ICommunityContactRepository
{
    /// <summary>Returns CommunityContact items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CommunityContactItemDto>> GetCommunityContactItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
