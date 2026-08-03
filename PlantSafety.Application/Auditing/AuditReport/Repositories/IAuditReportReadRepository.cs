// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the AuditReport read
//   model, mirroring the RiskManagement precedent of a separate Application-facing read
//   repository alongside the Domain-facing write repository.
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditReport.Models;

namespace PlantSafety.Application.Auditing.AuditReport.Repositories;

/// <summary>Read-only repository for the AuditReport read model. Implemented in Infrastructure.</summary>
public interface IAuditReportReadRepository
{
    /// <summary>Returns a single report by id, scoped to the tenant, or null.</summary>
    Task<AuditReportDto?> GetByIdAsync(Guid tenantId, Guid auditReportId, CancellationToken cancellationToken);

    /// <summary>Returns the most recent reports generated for a target entity, most recent first.</summary>
    Task<IReadOnlyList<AuditReportDto>> GetForTargetAsync(Guid tenantId, string targetEntityType, Guid targetEntityId, CancellationToken cancellationToken);
}
