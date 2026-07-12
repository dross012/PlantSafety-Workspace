// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the InternalSafetyAudit read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Auditing.InternalSafetyAudit.Models;

namespace PlantSafety.Application.Compliance.Auditing.InternalSafetyAudit.Repositories;

/// <summary>Read-only repository for InternalSafetyAudit data. Implemented in the Infrastructure layer.</summary>
public interface IInternalSafetyAuditRepository
{
    /// <summary>Returns InternalSafetyAudit items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<InternalSafetyAuditItemDto>> GetAuditsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
