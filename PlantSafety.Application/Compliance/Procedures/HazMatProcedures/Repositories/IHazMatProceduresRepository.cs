// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the HazMatProcedures read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.HazMatProcedures.Models;

namespace PlantSafety.Application.Compliance.Procedures.HazMatProcedures.Repositories;

/// <summary>Read-only repository for HazMatProcedures data. Implemented in the Infrastructure layer.</summary>
public interface IHazMatProceduresRepository
{
    /// <summary>Returns HazMatProcedures items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HazMatProceduresItemDto>> GetHazMatProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
