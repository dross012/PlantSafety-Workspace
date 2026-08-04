// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the CommunityConcern read-model.
// SchemaVersion: 1

using PlantSafety.Application.CommunitySafety.CommunityConcern.Models;

namespace PlantSafety.Application.CommunitySafety.CommunityConcern.Repositories;

/// <summary>Read-only repository for CommunityConcern data. Implemented in the Infrastructure layer.</summary>
public interface ICommunityConcernRepository
{
    /// <summary>Returns open (not Resolved/Archived) CommunityConcern items for the tenant/site.</summary>
    Task<IReadOnlyList<CommunityConcernItemDto>> GetOpenCommunityConcernItemsAsync(
        Guid tenantId,
        Guid? siteId,
        CancellationToken cancellationToken);

    /// <summary>Returns a single CommunityConcern item by identity, or null if not found.</summary>
    Task<CommunityConcernItemDto?> GetCommunityConcernItemByIdAsync(
        Guid tenantId,
        Guid communityConcernId,
        CancellationToken cancellationToken);
}
