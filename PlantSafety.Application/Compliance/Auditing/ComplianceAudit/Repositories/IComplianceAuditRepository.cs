// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the ComplianceAudit read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Auditing.ComplianceAudit.Models;

namespace PlantSafety.Application.Compliance.Auditing.ComplianceAudit.Repositories;

/// <summary>Read-only repository for ComplianceAudit data. Implemented in the Infrastructure layer.</summary>
public interface IComplianceAuditRepository
{
    /// <summary>Returns ComplianceAudit items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ComplianceAuditItemDto>> GetAuditsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
