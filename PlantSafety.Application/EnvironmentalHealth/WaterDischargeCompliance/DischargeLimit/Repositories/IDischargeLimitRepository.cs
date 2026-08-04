// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only repository abstraction for DischargeLimit data. Implemented in the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Models;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Repositories;

/// <summary>Read-only repository for DischargeLimit data. Implemented in the Infrastructure layer.</summary>
public interface IDischargeLimitRepository
{
    /// <summary>Backs ListDischargeLimitsByPointQuery.</summary>
    Task<IReadOnlyList<DischargeLimitItemDto>> ListDischargeLimitsByPointAsync(Guid tenantId, Guid dischargePointId, CancellationToken cancellationToken);
}