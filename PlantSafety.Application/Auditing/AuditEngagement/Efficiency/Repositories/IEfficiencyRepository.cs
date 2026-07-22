// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Efficiency read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.Efficiency.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.Efficiency.Repositories;

/// <summary>Read-only repository for Efficiency data. Implemented in the Infrastructure layer.</summary>
public interface IEfficiencyRepository
{
    /// <summary>Returns Efficiency items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EfficiencyItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}