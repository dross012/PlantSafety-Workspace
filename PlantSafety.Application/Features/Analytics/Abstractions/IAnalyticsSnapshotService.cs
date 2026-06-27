// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Abstractions for the Analytics module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Analytics.Abstractions;

/// <summary>Triggers an on-demand refresh of the analytics snapshot for a tenant.</summary>
public interface IAnalyticsSnapshotService
{
    /// <summary>Enqueues a background refresh of the dashboard snapshot for the given tenant and site.</summary>
    Task EnqueueSnapshotRefreshAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);
}

/// <summary>Computes the safety culture index score from raw survey and event data.</summary>
public interface ISafetyCultureIndexService
{
    /// <summary>
    /// Returns the computed culture index score (0–10) for the given tenant.
    /// Returns <c>null</c> when insufficient data is available.
    /// </summary>
    Task<decimal?> ComputeScoreAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);
}
