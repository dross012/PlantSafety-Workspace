// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the IncidentManagement read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.IncidentManagement.Models;

namespace PlantSafety.Application.SafetyEvents.IncidentManagement.Repositories;

/// <summary>Read-only repository for IncidentManagement data. Implemented in the Infrastructure layer.</summary>
public interface IIncidentManagementRepository
{
    /// <summary>Returns IncidentManagement items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<IncidentManagementItemDto>> GetIncidentsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
