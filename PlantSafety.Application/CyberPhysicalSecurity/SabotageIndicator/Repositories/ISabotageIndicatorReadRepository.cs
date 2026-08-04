// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only repository abstraction for the SabotageIndicator read-model.
//   Implemented in the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Models;

namespace PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Repositories;

/// <summary>Read-only repository for SabotageIndicator data. Implemented in the Infrastructure layer.</summary>
public interface ISabotageIndicatorReadRepository
{
    /// <summary>Returns all active (Open/UnderInvestigation) SabotageIndicators for a tenant. Backs the Active Sabotage Indicator Board.</summary>
    Task<IReadOnlyList<SabotageIndicatorDto>> ListActiveAsync(Guid tenantId, CancellationToken cancellationToken);
}
