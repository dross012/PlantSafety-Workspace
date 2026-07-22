// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Operations read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.Operations.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.Operations.Repositories;

/// <summary>Read-only repository for Operations data. Implemented in the Infrastructure layer.</summary>
public interface IOperationsRepository
{
    /// <summary>Returns Operations items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<OperationsItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}