// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Procedures read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.Procedures.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.Procedures.Repositories;

/// <summary>Read-only repository for Procedures data. Implemented in the Infrastructure layer.</summary>
public interface IProceduresRepository
{
    /// <summary>Returns Procedures items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ProceduresItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}