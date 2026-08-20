// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Application
//  File:           FeatureSetDetailDto.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single FeatureSet record, including every
//                  entity property plus an ETag. Backs GetFeatureSetByIdQuery. Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Models.DataLakeActivityDetailDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Intelligence.FeatureEngineering.Models;

/// <summary>A single FeatureSet read-model record.</summary>
public sealed record FeatureSetDetailDto
{
    /// <summary>Feature set identifier.</summary>
    public required Guid FeatureSetId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>The name of this feature set (e.g., "IncidentRiskFeatures").</summary>
    public required string Name { get; init; }

    /// <summary>The version of this feature set ("MAJOR.MINOR").</summary>
    public required string Version { get; init; }

    /// <summary>The masked data source path used to compute these features.</summary>
    public required string DataSourcePath { get; init; }

    /// <summary>Whether this feature set has been published to the feature store.</summary>
    public required bool IsPublished { get; init; }

    /// <summary>Feature names currently on this set, comma-delimited.</summary>
    public required string FeatureNamesCsv { get; init; }

    /// <summary>Count of features currently on this set.</summary>
    public required int FeatureCount { get; init; }

    /// <summary>UTC timestamp when this feature set was computed.</summary>
    public required DateTime ComputedAtUtc { get; init; }

    /// <summary>UTC timestamp when this feature set was published. Null until published.</summary>
    public DateTime? PublishedAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
