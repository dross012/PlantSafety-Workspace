// ============================================================================
// File: ModelRegistryCommandRequests.cs
// Project: PlantSafety.Api
// Module: Intelligence.ModelRegistry
// Created: 2026-08-20
// Description: Request DTOs bound from the HTTP request body for the Intelligence.ModelRegistry
//              V1 controller's Register/SubmitForApproval/Approve/Deprecate/Retire lifecycle
//              actions. Mirrors PlantSafety.Api.Intelligence.DataLake.Contracts.Requests
//              .DataLakeActivityCommandRequests.cs -- every mutating lifecycle request carries
//              an optional ETag field so clients can supply the optimistic-concurrency token
//              captured from a prior read (e.g. ModelRegistryEntryDetailDto.ETag); a stale value
//              results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.Intelligence.ModelRegistry.Contracts.Requests;

/// <summary>Request body for registering a new model version in the model registry.</summary>
public sealed record RegisterModelRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>The logical model name.</summary>
    public required string ModelName { get; init; }

    /// <summary>The semantic version of this model ("MAJOR.MINOR.PATCH").</summary>
    public required string ModelVersion { get; init; }

    /// <summary>SHA-256 hash of the training dataset, for lineage tracking.</summary>
    public required string DatasetHash { get; init; }

    /// <summary>Primary accuracy score from the training job (0.0-1.0).</summary>
    public required double TrainingAccuracyScore { get; init; }
}

/// <summary>Request body for submitting a Draft model registry entry for human approval (Draft -&gt; PendingApproval).</summary>
public sealed record SubmitModelForApprovalRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this entry. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>
/// SAFETY-CRITICAL (Safety Rule 4.5) request body for approving a PendingApproval model registry
/// entry for production deployment (PendingApproval -&gt; Approved). The approving user is always
/// resolved server-side from the authenticated actor -- never accepted from the request body --
/// so the human-approver invariant cannot be spoofed by the caller.
/// </summary>
public sealed record ApproveModelRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this entry. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for deprecating an Approved model registry entry (Approved -&gt; Deprecated).</summary>
public sealed record DeprecateModelRequest
{
    /// <summary>Reason the model is being deprecated (e.g., superseded by a newer approved version).</summary>
    public required string Reason { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this entry. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for permanently retiring a model registry entry (terminal).</summary>
public sealed record RetireModelRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this entry. Null skips the check.</summary>
    public string? ETag { get; init; }
}
