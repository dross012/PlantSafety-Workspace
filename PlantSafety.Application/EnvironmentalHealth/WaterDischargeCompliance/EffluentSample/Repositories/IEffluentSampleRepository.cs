// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only repository abstraction for EffluentSample data. Implemented in the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Models;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Repositories;

/// <summary>Read-only repository for EffluentSample data. Implemented in the Infrastructure layer.</summary>
public interface IEffluentSampleRepository
{
    /// <summary>Backs GetSampleHistoryByPointQuery.</summary>
    Task<IReadOnlyList<EffluentSampleItemDto>> GetSampleHistoryByPointAsync(Guid tenantId, Guid dischargePointId, DateTime from, DateTime to, CancellationToken cancellationToken);
}