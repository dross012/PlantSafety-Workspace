// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-08-02 -- Phase 4 full build-out: added GetByIdAsync, backing the new
//          GetCrisisManagementByIdQuery/CrisisManagementDetailDto read-before-write flow, so a
//          caller can retrieve a single record's ETag before issuing a lifecycle command. Extends
//          this pre-existing interface (rather than adding a second, colliding
//          ICrisisManagementRepository under this same Application namespace) per this codebase's
//          IPermitsRepository precedent.
// FILE DESCRIPTION: Repository abstraction for the CrisisManagement read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.CrisisManagement.Models;

namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Repositories;

/// <summary>Read-only repository for CrisisManagement data. Implemented in the Infrastructure layer.</summary>
public interface ICrisisManagementRepository
{
    /// <summary>
    /// Returns the CrisisManagement read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<CrisisManagementDetailDto?> GetByIdAsync(Guid crisisManagementId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns CrisisManagement items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CrisisManagementItemDto>> GetCrisisEventsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
