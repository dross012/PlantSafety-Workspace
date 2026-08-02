// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-29
// FILE DESCRIPTION: Read-only repository abstraction for the Risk read-model. Implemented in the
//   Infrastructure layer. Distinct from PlantSafety.Domain.RiskManagement.Interfaces.IRiskRepository
//   (the write-side aggregate repository) -- same name, different namespace, mirroring
//   PlantSafety.Application.PermitToWork.PermitWorkflow.Repositories.IPermitWorkflowRepository's
//   precedent for CQRS read/write repository separation in this codebase.
// SchemaVersion: 1
// UPDATED: 2026-07-29 -- GetByIdAsync now requires tenantId, pushed down to usp_Risk_GetById's
//   WHERE clause (Phase 4 cross-tenant access remediation), rather than the caller fetching by
//   RiskId alone and checking TenantId in application code after the fact.

using PlantSafety.Application.RiskManagement.Models;

namespace PlantSafety.Application.RiskManagement.Repositories;

/// <summary>Read-only repository for Risk data. Implemented in the Infrastructure layer.</summary>
public interface IRiskRepository
{
    /// <summary>
    /// Returns the Risk read-model record for the given identity, scoped to the given tenant, or
    /// <see langword="null"/> if not found or if the Risk belongs to a different tenant.
    /// </summary>
    Task<RiskItemDto?> GetByIdAsync(Guid riskId, Guid tenantId, CancellationToken cancellationToken);
}
