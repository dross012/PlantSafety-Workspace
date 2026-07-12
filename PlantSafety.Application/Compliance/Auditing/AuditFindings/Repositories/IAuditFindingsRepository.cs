// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the AuditFindings read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Auditing.AuditFindings.Models;

namespace PlantSafety.Application.Compliance.Auditing.AuditFindings.Repositories;

/// <summary>Read-only repository for AuditFindings data. Implemented in the Infrastructure layer.</summary>
public interface IAuditFindingsRepository
{
    /// <summary>Returns AuditFindings items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<AuditFindingItemDto>> GetFindingsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
