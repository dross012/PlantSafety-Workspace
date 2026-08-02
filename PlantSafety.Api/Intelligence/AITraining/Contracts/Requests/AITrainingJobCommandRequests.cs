// ============================================================================
// File: AITrainingJobCommandRequests.cs
// Project: PlantSafety.Api
// Module: Intelligence.AITraining
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body for the Intelligence.AITraining V1
//              controller's Phase 4 full build-out lifecycle actions
//              (Create -> Start -> Complete/Fail -> Archive). Mirrors
//              PlantSafety.Api.ContinuousImprovement.RCA.Contracts.Requests.RCACommandRequests.cs
//              -- every mutating lifecycle request carries an optional ETag field so clients can
//              supply the optimistic-concurrency token captured from a prior read (e.g.
//              AITrainingJobDetailDto.ETag); a stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.Intelligence.AITraining.Contracts.Requests;

/// <summary>Request body for creating a new Intelligence.AITraining training job record.</summary>
public sealed record CreateAITrainingJobRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the model being trained.</summary>
    public required string ModelName { get; init; }

    /// <summary>Version identifier of the trained model artifact.</summary>
    public required string ModelVersion { get; init; }

    /// <summary>Reference to the training dataset used for this run.</summary>
    public string? DatasetReference { get; init; }
}

/// <summary>Request body for beginning execution of a training job (Queued -&gt; Running).</summary>
public sealed record StartAITrainingJobRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this training job record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for marking a training job as completed successfully (Running -&gt; Completed).</summary>
public sealed record CompleteAITrainingJobRequest
{
    /// <summary>Reference to the produced model artifact, if any.</summary>
    public string? ModelArtifactReference { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this training job record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for marking a training job as failed (Running -&gt; Failed).</summary>
public sealed record FailAITrainingJobRequest
{
    /// <summary>Reason the training job failed.</summary>
    public required string FailureReason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this training job record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a completed or failed training job for compliance retention (Completed/Failed -&gt; Archived).</summary>
public sealed record ArchiveAITrainingJobRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this training job record. Null skips the check.</summary>
    public string? ETag { get; init; }
}
