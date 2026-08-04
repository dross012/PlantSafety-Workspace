// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           TaskComplexityProfileDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO backing GetTaskComplexityProfileQuery. Backs the Task
//                  Complexity vs. Load Correlation dashboard. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;

/// <summary>A single task-complexity profile read-model record.</summary>
public sealed record TaskComplexityProfileDto
{
    /// <summary>Profile identifier.</summary>
    public required Guid TaskComplexityProfileId { get; init; }

    /// <summary>Owning ledger identifier.</summary>
    public required Guid CognitiveLoadLedgerId { get; init; }

    /// <summary>Task business code.</summary>
    public required string TaskCode { get; init; }

    /// <summary>Task name.</summary>
    public required string TaskName { get; init; }

    /// <summary>Complexity rating, 1-10.</summary>
    public int ComplexityScore { get; init; }

    /// <summary>Narrative description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>UTC instant this profile was defined.</summary>
    public DateTime DefinedAtUtc { get; init; }
}
