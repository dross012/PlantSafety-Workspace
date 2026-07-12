// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO for a single AI/ML model training job run.
// SchemaVersion: 1
namespace PlantSafety.Application.Intelligence.AITraining.Models;

/// <summary>Represents a single AI/ML model training job run.</summary>
public sealed record AITrainingItemDto
{
    /// <summary>Unique identifier of the training job run.</summary>
    public required Guid TrainingJobId { get; init; }

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

    /// <summary>Current training job status (e.g., Queued/Running/Completed/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the training job started.</summary>
    public required DateTime StartedAtUtc { get; init; }

    /// <summary>Identity of the user or process that initiated the training job.</summary>
    public required Guid InitiatedBy { get; init; }
}