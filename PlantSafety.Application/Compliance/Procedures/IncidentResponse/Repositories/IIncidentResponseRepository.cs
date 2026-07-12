// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the IncidentResponse read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.IncidentResponse.Models;

namespace PlantSafety.Application.Compliance.Procedures.IncidentResponse.Repositories;

/// <summary>Read-only repository for IncidentResponse data. Implemented in the Infrastructure layer.</summary>
public interface IIncidentResponseRepository
{
    /// <summary>Returns IncidentResponse items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<IncidentResponseItemDto>> GetIncidentResponseProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
