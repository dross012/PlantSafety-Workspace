// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the ErgonomicsWorkstationDesign read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.ErgonomicsWorkstationDesign.Models;

namespace PlantSafety.Application.SafetyProcedures.ErgonomicsWorkstationDesign.Repositories;

/// <summary>Read-only repository for ErgonomicsWorkstationDesign data. Implemented in the Infrastructure layer.</summary>
public interface IErgonomicsWorkstationDesignRepository
{
    /// <summary>Returns ErgonomicsWorkstationDesign items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ErgonomicsWorkstationDesignItemDto>> GetErgonomicsWorkstationDesignItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}