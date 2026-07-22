// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the LotoProcedures read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.LotoProcedures.Models;

namespace PlantSafety.Application.SafetyProcedures.LotoProcedures.Repositories;

/// <summary>Read-only repository for LotoProcedures data. Implemented in the Infrastructure layer.</summary>
public interface ILotoProceduresRepository
{
    /// <summary>Returns LotoProcedures items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<LotoProceduresItemDto>> GetLotoProceduresItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}