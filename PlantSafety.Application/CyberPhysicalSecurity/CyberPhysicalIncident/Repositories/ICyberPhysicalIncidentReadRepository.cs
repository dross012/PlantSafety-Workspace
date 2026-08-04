// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only repository abstraction for the CyberPhysicalIncident read-model.
//   Implemented in the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Models;

namespace PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Repositories;

/// <summary>Read-only repository for CyberPhysicalIncident data. Implemented in the Infrastructure layer.</summary>
public interface ICyberPhysicalIncidentReadRepository
{
    /// <summary>Returns the CyberPhysicalIncident read-model record for the given identity and tenant, or <see langword="null"/> if not found.</summary>
    Task<CyberPhysicalIncidentDto?> GetByIdAsync(Guid cyberPhysicalIncidentId, Guid tenantId, CancellationToken cancellationToken);
}
