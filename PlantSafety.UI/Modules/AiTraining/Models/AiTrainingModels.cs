// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the AI Training module. Mirrors the
//   real PlantSafety.Api read-model shapes (AITrainingItemDto / AITrainingResponse
//   at GET v1/intelligence/aitraining) so AiTrainingApiClient can deserialize
//   real backend responses directly. No pagination — the endpoint returns the
//   full item set for the requested [From, To] window.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.AiTraining.Models;

/// <summary>Represents a single AI/ML model training job run (mirrors <c>AITrainingItemDto</c>).</summary>
public sealed class AiTrainingItemDto
{
    /// <summary>Gets the unique identifier of the training job run.</summary>
    public Guid TrainingJobId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the name of the model being trained.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>Gets the version identifier of the trained model artifact.</summary>
    public string ModelVersion { get; init; } = string.Empty;

    /// <summary>Gets the reference to the training dataset used for this run.</summary>
    public string? DatasetReference { get; init; }

    /// <summary>Gets the current training job status (e.g., Queued/Running/Completed/Failed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the training job started.</summary>
    public DateTime StartedAtUtc { get; init; }

    /// <summary>Gets the identity of the user or process that initiated the training job.</summary>
    public Guid InitiatedBy { get; init; }
}

/// <summary>Envelope returned by GET v1/intelligence/aitraining (mirrors <c>AITrainingResponse</c>).</summary>
public sealed class AiTrainingResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the training job items in the requested window.</summary>
    public IReadOnlyList<AiTrainingItemDto> Items { get; init; } = [];
}
