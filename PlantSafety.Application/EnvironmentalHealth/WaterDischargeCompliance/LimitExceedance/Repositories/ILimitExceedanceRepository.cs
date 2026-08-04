// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only repository abstraction for LimitExceedance data. Implemented in the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Models;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Repositories;

/// <summary>Read-only repository for LimitExceedance data. Implemented in the Infrastructure layer.</summary>
public interface ILimitExceedanceRepository
{
    /// <summary>Backs ListActiveExceedancesQuery.</summary>
    Task<IReadOnlyList<LimitExceedanceItemDto>> ListActiveExceedancesAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);
}