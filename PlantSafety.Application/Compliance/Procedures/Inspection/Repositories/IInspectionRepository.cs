// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Inspection read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.Inspection.Models;

namespace PlantSafety.Application.Compliance.Procedures.Inspection.Repositories;

/// <summary>Read-only repository for Inspection data. Implemented in the Infrastructure layer.</summary>
public interface IInspectionRepository
{
    /// <summary>Returns Inspection items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<InspectionItemDto>> GetInspectionProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
