// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Application
//  File:           ModelRegistryEntryDetailDto.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single ModelRegistryEntry record, including
//                  every aggregate property plus an ETag. Backs GetModelRegistryEntryByIdQuery.
//                  Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Models.DataLakeActivityDetailDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Intelligence.ModelRegistry.Models;

/// <summary>A single ModelRegistryEntry read-model record, including the full governance lifecycle state.</summary>
public sealed record ModelRegistryEntryDetailDto
{
    /// <summary>Model registry entry identifier.</summary>
    public required Guid ModelRegistryEntryId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>The logical model name.</summary>
    public required string ModelName { get; init; }

    /// <summary>The semantic version of this model.</summary>
    public required string ModelVersion { get; init; }

    /// <summary>Current governance status (Draft/PendingApproval/Approved/Deprecated/Retired).</summary>
    public required string Status { get; init; }

    /// <summary>SHA-256 hash of the training dataset. Used for lineage tracking.</summary>
    public required string DatasetHash { get; init; }

    /// <summary>Primary accuracy score from the training job (0.0-1.0).</summary>
    public required double TrainingAccuracyScore { get; init; }

    /// <summary>Whether this entry currently meets the platform's minimum confidence threshold for production deployment.</summary>
    public required bool MeetsMinimumConfidenceThreshold { get; init; }

    /// <summary>Identity of the user or process that registered this record.</summary>
    public required Guid RegisteredBy { get; init; }

    /// <summary>The human actor who approved this model. Null until approved.</summary>
    public Guid? ApprovedBy { get; init; }

    /// <summary>UTC timestamp when approved. Null until approved.</summary>
    public DateTime? ApprovedAtUtc { get; init; }

    /// <summary>Sanitized deprecation reason. Populated when deprecated.</summary>
    public string? DeprecationReason { get; init; }

    /// <summary>UTC instant the entry was deprecated.</summary>
    public DateTime? DeprecatedAtUtc { get; init; }

    /// <summary>UTC instant the entry was retired.</summary>
    public DateTime? RetiredAtUtc { get; init; }

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
