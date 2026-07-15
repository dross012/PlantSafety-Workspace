// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Job Detail page (Volume S1 §2):
//   job summary/metadata, timeline steps, constraints, and predictive
//   scheduling insights. Display-only — no domain logic.
// SchemaVersion: 2026.1
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Scheduling.Models;

/// <summary>Full job detail (summary + metadata header).</summary>
public sealed class JobDetailDto
{
    /// <summary>Gets the job identifier (UUIDv7).</summary>
    public Guid JobId { get; init; }

    /// <summary>Gets the human-readable job code (format J-###).</summary>
    public string JobCode { get; init; } = string.Empty;

    /// <summary>Gets the job status code.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the assigned resource display name.</summary>
    public string ResourceName { get; init; } = string.Empty;

    /// <summary>Gets the resource identifier.</summary>
    public Guid ResourceId { get; init; }

    /// <summary>Gets the scheduled duration in minutes.</summary>
    public int DurationMinutes { get; init; }

    /// <summary>Gets the UTC timestamp the job was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp the job was last updated.</summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }

    /// <summary>Gets the display name of the operator who owns this job.</summary>
    public string OperatorName { get; init; } = string.Empty;
}

/// <summary>Single timeline step row.</summary>
public sealed class TimelineStepDto
{
    /// <summary>Gets the strictly-increasing step number.</summary>
    public int StepNumber { get; init; }

    /// <summary>Gets the step description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the step start time (UTC).</summary>
    public DateTimeOffset StartUtc { get; init; }

    /// <summary>Gets the step end time (UTC).</summary>
    public DateTimeOffset EndUtc { get; init; }

    /// <summary>Gets the step status code.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Single scheduling constraint row.</summary>
public sealed class ConstraintDto
{
    /// <summary>Gets the constraint type (Machine, Shift, Dependency, Safety, Capacity, Setup).</summary>
    public string ConstraintType { get; init; } = string.Empty;

    /// <summary>Gets a human-readable description of the constraint.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether the constraint currently passes.</summary>
    public bool IsSatisfied { get; init; }
}

/// <summary>Predictive scheduling model output for a single job.</summary>
public sealed class PredictiveInsightsDto
{
    /// <summary>Gets the normalized risk score (0-1).</summary>
    public double RiskScore { get; init; }

    /// <summary>Gets the model-recommended start time (UTC).</summary>
    public DateTimeOffset RecommendedStartUtc { get; init; }

    /// <summary>Gets the model-recommended end time (UTC).</summary>
    public DateTimeOffset RecommendedEndUtc { get; init; }

    /// <summary>Gets the traceable model version identifier.</summary>
    public string ModelVersion { get; init; } = string.Empty;
}
