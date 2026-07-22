// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the QualityControl read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.QualityControl.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.QualityControl.Repositories;

/// <summary>Read-only repository for QualityControl data. Implemented in the Infrastructure layer.</summary>
public interface IQualityControlRepository
{
    /// <summary>Returns QualityControl items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<QualityControlItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}