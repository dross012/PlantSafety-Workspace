// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Inspecting read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Auditing.AuditEngagement.Inspecting.Models;

namespace PlantSafety.Application.Auditing.AuditEngagement.Inspecting.Repositories;

/// <summary>Read-only repository for Inspecting data. Implemented in the Infrastructure layer.</summary>
public interface IInspectingRepository
{
    /// <summary>Returns Inspecting items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<InspectingItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}