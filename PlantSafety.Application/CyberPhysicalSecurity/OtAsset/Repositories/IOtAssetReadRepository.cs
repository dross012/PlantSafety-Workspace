// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only repository abstraction for the OtAsset read-model. Implemented in
//   the Infrastructure layer. Distinct from
//   PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Interfaces.IOtAssetRepository (the write-side
//   aggregate repository), mirroring the RiskManagement CQRS read/write repository-separation
//   precedent.
// SchemaVersion: 1

using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Models;

namespace PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Repositories;

/// <summary>Read-only repository for OtAsset data. Implemented in the Infrastructure layer.</summary>
public interface IOtAssetReadRepository
{
    /// <summary>Returns the OtAsset read-model record for the given identity and tenant, or <see langword="null"/> if not found.</summary>
    Task<OtAssetDto?> GetByIdAsync(Guid otAssetId, Guid tenantId, CancellationToken cancellationToken);
}
