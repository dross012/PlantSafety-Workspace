// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the LineBreak read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.LineBreak.Models;

namespace PlantSafety.Application.Compliance.Permits.LineBreak.Repositories;

/// <summary>Read-only repository for LineBreak data. Implemented in the Infrastructure layer.</summary>
public interface ILineBreakRepository
{
    /// <summary>Returns LineBreak items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<LineBreakItemDto>> GetLineBreakAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
