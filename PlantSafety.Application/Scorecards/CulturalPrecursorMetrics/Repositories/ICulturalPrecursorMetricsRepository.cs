// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Cultural & Precursor Metrics (Near-Miss, Behavioral Observations) read-model.
// SchemaVersion: 1

using PlantSafety.Application.Scorecards.CulturalPrecursorMetrics.Models;

namespace PlantSafety.Application.Scorecards.CulturalPrecursorMetrics.Repositories;

/// <summary>Read-only repository for Cultural & Precursor Metrics (Near-Miss, Behavioral Observations) data. Implemented in the Infrastructure layer.</summary>
public interface ICulturalPrecursorMetricsRepository
{
    /// <summary>Returns Cultural & Precursor Metrics (Near-Miss, Behavioral Observations) items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CulturalPrecursorMetricsItemDto>> GetCulturalPrecursorMetricsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}