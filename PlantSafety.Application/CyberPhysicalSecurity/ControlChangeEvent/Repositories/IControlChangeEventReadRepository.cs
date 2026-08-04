// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only repository abstraction for the ControlChangeEvent read-model.
//   Implemented in the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Models;

namespace PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Repositories;

/// <summary>Read-only repository for ControlChangeEvent data. Implemented in the Infrastructure layer.</summary>
public interface IControlChangeEventReadRepository
{
    /// <summary>Returns ControlChangeEvent records flagged Unauthorized (or PendingReview -&gt; Unauthorized) for a tenant, most recent first. Backs the Control-Change Audit Feed dashboard.</summary>
    Task<IReadOnlyList<ControlChangeEventDto>> ListUnauthorizedAsync(Guid tenantId, CancellationToken cancellationToken);
}
