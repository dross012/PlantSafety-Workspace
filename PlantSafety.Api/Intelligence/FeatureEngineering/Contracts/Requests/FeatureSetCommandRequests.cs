// ============================================================================
// File: FeatureSetCommandRequests.cs
// Project: PlantSafety.Api
// Module: Intelligence.FeatureEngineering
// Created: 2026-08-20
// Description: Request DTOs bound from the HTTP request body for the Intelligence
//              .FeatureEngineering V1 controller's Create/AddFeature/Publish lifecycle actions.
//              Mirrors PlantSafety.Api.Intelligence.DataLake.Contracts.Requests
//              .DataLakeActivityCommandRequests.cs -- every mutating lifecycle request carries
//              an optional ETag field so clients can supply the optimistic-concurrency token
//              captured from a prior read (e.g. FeatureSetDetailDto.ETag); a stale value results
//              in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.Intelligence.FeatureEngineering.Contracts.Requests;

/// <summary>Request body for creating a new, unpublished feature set.</summary>
public sealed record CreateFeatureSetRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>The name of this feature set (e.g., "IncidentRiskFeatures").</summary>
    public required string Name { get; init; }

    /// <summary>The version of this feature set ("MAJOR.MINOR"). Defaults to "1.0" when omitted.</summary>
    public string? Version { get; init; }

    /// <summary>The masked data source path used to compute these features.</summary>
    public required string DataSourcePath { get; init; }
}

/// <summary>Request body for adding a named feature to an unpublished feature set.</summary>
public sealed record AddFeatureToFeatureSetRequest
{
    /// <summary>Name of the feature to add.</summary>
    public required string FeatureName { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this feature set record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for publishing a feature set to the feature store (immutable thereafter).</summary>
public sealed record PublishFeatureSetRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this feature set record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
