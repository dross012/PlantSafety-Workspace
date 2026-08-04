// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only repository abstraction for the SensorIntegrityCheck read-model.
//   Implemented in the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Models;

namespace PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Repositories;

/// <summary>Read-only repository for SensorIntegrityCheck data. Implemented in the Infrastructure layer.</summary>
public interface ISensorIntegrityCheckReadRepository
{
    /// <summary>Returns the full integrity-check history for a given OT asset, most recent first.</summary>
    Task<IReadOnlyList<SensorIntegrityCheckDto>> GetHistoryForAssetAsync(Guid otAssetId, Guid tenantId, CancellationToken cancellationToken);
}
