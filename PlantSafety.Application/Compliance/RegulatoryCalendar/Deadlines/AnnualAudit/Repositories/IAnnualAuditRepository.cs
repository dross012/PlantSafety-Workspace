// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the AnnualAudit read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Models;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Repositories;

/// <summary>Read-only repository for AnnualAudit data. Implemented in the Infrastructure layer.</summary>
public interface IAnnualAuditRepository
{
    /// <summary>Returns AnnualAudit items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<AnnualAuditItemDto>> GetDeadlinesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
