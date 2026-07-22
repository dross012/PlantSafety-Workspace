// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Systems read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.Systems.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.Systems.Repositories;

/// <summary>Read-only repository for Systems data. Implemented in the Infrastructure layer.</summary>
public interface ISystemsRepository
{
    /// <summary>Returns Systems items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SystemsItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}