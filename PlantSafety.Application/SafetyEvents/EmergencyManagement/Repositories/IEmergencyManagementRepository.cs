// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-08-02 -- Phase 4 full build-out: added GetByIdAsync, backing the new
//          GetEmergencyManagementByIdQuery read path via usp_EmergencyManagementItems_GetById.
// FILE DESCRIPTION: Repository abstraction for the EmergencyManagement read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.EmergencyManagement.Models;

namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Repositories;

/// <summary>Read-only repository for EmergencyManagement data. Implemented in the Infrastructure layer.</summary>
public interface IEmergencyManagementRepository
{
    /// <summary>Returns EmergencyManagement items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmergencyManagementItemDto>> GetEmergenciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the EmergencyManagement read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if the record belongs to a
    /// different tenant.
    /// </summary>
    Task<EmergencyManagementItemDto?> GetByIdAsync(Guid emergencyManagementId, Guid tenantId, CancellationToken cancellationToken);
}
