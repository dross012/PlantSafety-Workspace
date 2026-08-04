// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only repository abstraction for DischargePoint data. Implemented in the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Models;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Repositories;

/// <summary>Read-only repository for DischargePoint data. Implemented in the Infrastructure layer.</summary>
public interface IDischargePointRepository
{
    /// <summary>Backs ListDischargePointsBySiteQuery.</summary>
    Task<IReadOnlyList<DischargePointItemDto>> ListDischargePointsBySiteAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);
}