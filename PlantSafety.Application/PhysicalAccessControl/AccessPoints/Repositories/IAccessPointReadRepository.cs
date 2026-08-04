// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the AccessPoint read-model.
// SchemaVersion: 1

using PlantSafety.Application.PhysicalAccessControl.AccessPoints.Models;

namespace PlantSafety.Application.PhysicalAccessControl.AccessPoints.Repositories;

/// <summary>Read-only repository for AccessPoint data. Implemented in the Infrastructure layer.</summary>
public interface IAccessPointReadRepository
{
    /// <summary>Returns a single AccessPoint by identity, scoped to the tenant, or null if not found.</summary>
    Task<AccessPointDto?> GetByIdAsync(Guid tenantId, Guid accessPointId, CancellationToken cancellationToken);
}
