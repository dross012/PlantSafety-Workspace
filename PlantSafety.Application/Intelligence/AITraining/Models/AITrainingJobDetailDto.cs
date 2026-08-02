// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Application
//  File:           AITrainingJobDetailDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single AITrainingJob record, including every
//                  aggregate property plus an ETag. Backs GetAITrainingJobByIdQuery. Distinct from
//                  the pre-existing, simpler AITrainingItemDto used by the date-range list query
//                  (GetAITrainingQuery), which is left entirely untouched by this pass. Named
//                  "...DetailDto" rather than "AITrainingItemDto" (already taken) -- mirroring the
//                  identical precedent already established by
//                  PlantSafety.Application.ContinuousImprovement.RCA.Models.RCADetailDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Intelligence.AITraining.Models;

/// <summary>A single AITrainingJob read-model record, including the full lifecycle state.</summary>
public sealed record AITrainingJobDetailDto
{
    /// <summary>Training job identifier.</summary>
    public required Guid AITrainingJobId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the model being trained.</summary>
    public required string ModelName { get; init; }

    /// <summary>Version identifier of the trained model artifact.</summary>
    public required string ModelVersion { get; init; }

    /// <summary>Reference to the training dataset used for this run.</summary>
    public string? DatasetReference { get; init; }

    /// <summary>Current lifecycle status (Queued/Running/Completed/Failed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the training job started (or was queued to start).</summary>
    public required DateTime StartedAtUtc { get; init; }

    /// <summary>Identity of the user or process that initiated the training job.</summary>
    public required Guid InitiatedBy { get; init; }

    /// <summary>Identity of the user who started (transitioned to Running) the training job.</summary>
    public Guid? RunStartedBy { get; init; }

    /// <summary>UTC instant the training job transitioned to Running.</summary>
    public DateTime? RunStartedAtUtc { get; init; }

    /// <summary>Reference to the produced model artifact, if any, recorded on completion.</summary>
    public string? ModelArtifactReference { get; init; }

    /// <summary>UTC instant the training job completed successfully.</summary>
    public DateTime? CompletedAtUtc { get; init; }

    /// <summary>Failure reason recorded when the training job failed, if any.</summary>
    public string? FailureReason { get; init; }

    /// <summary>UTC instant the training job failed.</summary>
    public DateTime? FailedAtUtc { get; init; }

    /// <summary>UTC instant the training job was archived for compliance retention.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

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
