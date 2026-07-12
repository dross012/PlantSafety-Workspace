// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the EmergencyProcedure read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.EmergencyProcedure.Models;

namespace PlantSafety.Application.Compliance.Procedures.EmergencyProcedure.Repositories;

/// <summary>Read-only repository for EmergencyProcedure data. Implemented in the Infrastructure layer.</summary>
public interface IEmergencyProcedureRepository
{
    /// <summary>Returns EmergencyProcedure items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmergencyProcedureItemDto>> GetProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
