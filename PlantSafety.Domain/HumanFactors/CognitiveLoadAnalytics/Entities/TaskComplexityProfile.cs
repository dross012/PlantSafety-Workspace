// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           TaskComplexityProfile.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable child entity of CognitiveLoadLedger defining a task's complexity
//                  rating, used to correlate task complexity against measured cognitive load
//                  (Task Complexity vs. Load Correlation dashboard). Deliberately raises no domain
//                  event on definition -- mirrors AdvancedMetricsLedger.Create's "silent substrate"
//                  precedent; the first correlated reading is the observable signal.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Entities;

/// <summary>A single task-complexity rating captured on a <see cref="Aggregates.CognitiveLoadLedger"/>.</summary>
public sealed class TaskComplexityProfile
{
    /// <summary>Unique identity of this profile.</summary>
    public TaskComplexityProfileId TaskComplexityProfileId { get; private set; }

    /// <summary>Short business code identifying the task (e.g. "CTRL-ROOM-STARTUP").</summary>
    public string TaskCode { get; private set; } = string.Empty;

    /// <summary>Human-readable task name.</summary>
    public string TaskName { get; private set; } = string.Empty;

    /// <summary>Complexity rating on a 1-10 scale (higher = more cognitively demanding).</summary>
    public int ComplexityScore { get; private set; }

    /// <summary>Narrative description of what drives the task's complexity.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>UTC instant this profile was defined.</summary>
    public UtcTimestamp DefinedAtUtc { get; private set; }

    /// <summary>Identity of the user who defined this profile.</summary>
    public UserId DefinedBy { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private TaskComplexityProfile() { }

    /// <summary>Creates a new task complexity profile.</summary>
    public TaskComplexityProfile(
        TaskComplexityProfileId taskComplexityProfileId,
        string taskCode,
        string taskName,
        int complexityScore,
        string description,
        UtcTimestamp definedAtUtc,
        UserId definedBy)
    {
        if (string.IsNullOrWhiteSpace(taskCode))
            throw new ArgumentException("TaskCode must not be null or whitespace.", nameof(taskCode));
        if (string.IsNullOrWhiteSpace(taskName))
            throw new ArgumentException("TaskName must not be null or whitespace.", nameof(taskName));
        if (complexityScore is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(complexityScore), "ComplexityScore must be between 1 and 10.");

        TaskComplexityProfileId = taskComplexityProfileId;
        TaskCode                 = taskCode.Trim();
        TaskName                 = taskName.Trim();
        ComplexityScore          = complexityScore;
        Description              = (description ?? string.Empty).Trim();
        DefinedAtUtc              = definedAtUtc;
        DefinedBy                 = definedBy;
    }
}
