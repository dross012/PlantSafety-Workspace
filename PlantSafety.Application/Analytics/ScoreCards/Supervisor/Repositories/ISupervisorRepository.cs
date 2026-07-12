// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Supervisor read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.ScoreCards.Supervisor.Models;

namespace PlantSafety.Application.Analytics.ScoreCards.Supervisor.Repositories;

/// <summary>Read-only repository for Supervisor data. Implemented in the Infrastructure layer.</summary>
public interface ISupervisorRepository
{
    /// <summary>Returns Supervisor items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SupervisorItemDto>> GetSupervisorScoresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
