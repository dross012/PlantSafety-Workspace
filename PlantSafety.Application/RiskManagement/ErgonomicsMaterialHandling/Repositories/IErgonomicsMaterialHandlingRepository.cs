// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Ergonomics & Material Handling read-model.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.ErgonomicsMaterialHandling.Models;

namespace PlantSafety.Application.RiskManagement.ErgonomicsMaterialHandling.Repositories;

/// <summary>Read-only repository for Ergonomics &amp; Material Handling data. Implemented in the Infrastructure layer.</summary>
public interface IErgonomicsMaterialHandlingRepository
{
    /// <summary>Returns Ergonomics &amp; Material Handling items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ErgonomicsMaterialHandlingItemDto>> GetErgonomicsMaterialHandlingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}