// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           SafetyProgramGoal.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity representing a single named goal/objective owned by a
//                  SafetyProgram. Free-text by design -- the composed rollup of live
//                  policy/procedure/culture activity is computed separately (not stored) by
//                  GetSafetyProgramDetailQueryHandler so it never goes stale. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.Entities;

/// <summary>A single named goal/objective owned by a SafetyProgram.</summary>
public sealed class SafetyProgramGoal
{
    /// <summary>Identity of this goal within its owning SafetyProgram.</summary>
    public SafetyProgramGoalId SafetyProgramGoalId { get; private set; }

    /// <summary>Human-readable goal/objective text.</summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>Ordinal position among the program's goals, for stable display ordering.</summary>
    public int SortOrder { get; private set; }

    // For rehydration only.
    private SafetyProgramGoal() { }

    /// <summary>Creates a new SafetyProgramGoal.</summary>
    public static SafetyProgramGoal Create(string text, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Goal text must not be empty.", nameof(text));

        return new SafetyProgramGoal
        {
            SafetyProgramGoalId = SafetyProgramGoalId.New(),
            Text = text.Trim(),
            SortOrder = sortOrder,
        };
    }
}
