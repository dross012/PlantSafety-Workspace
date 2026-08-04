// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only repository abstraction for DischargePermit data. Implemented in the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Models;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Repositories;

/// <summary>Read-only repository for DischargePermit data. Implemented in the Infrastructure layer.</summary>
public interface IDischargePermitRepository
{
    /// <summary>Backs GetDischargePermitByIdQuery.</summary>
    Task<DischargePermitItemDto?> GetDischargePermitByIdAsync(Guid tenantId, Guid dischargePermitId, CancellationToken cancellationToken);
}