// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the SafetyCaseValidation read-model.
// SchemaVersion: 1

using PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Models;

namespace PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Repositories;

/// <summary>Read-only repository for SafetyCaseValidation data. Implemented in the Infrastructure layer.</summary>
public interface ISafetyCaseValidationRepository
{
    /// <summary>Returns the most recent safety-case status for the given autonomous vehicle asset, or <see langword="null"/> if none exists.</summary>
    Task<SafetyCaseValidationItemDto?> GetSafetyCaseStatusByAssetIdAsync(
        Guid tenantId,
        Guid autonomousVehicleAssetId,
        CancellationToken cancellationToken);
}
