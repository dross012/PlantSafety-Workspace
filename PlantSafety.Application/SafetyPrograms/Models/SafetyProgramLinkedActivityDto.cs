// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: A single composed rollup section within SafetyProgramDetailDto.LinkedActivity,
//   summarizing live data fetched from one sibling module's read-model query (SafetyPolicies,
//   SafetyProcedures, or SafetyCulture). Resilient/degradable per section, mirroring
//   PlantSafety.Application.Operations.SystemHealth.Models.SystemHealthSectionDto.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyPrograms.Models;

/// <summary>A single composed rollup section of a SafetyProgram's live linked activity.</summary>
public sealed record SafetyProgramLinkedActivityDto
{
    /// <summary>The sibling module this section was composed from (SafetyPolicies/SafetyProcedures/SafetyCulture).</summary>
    public required string SourceModule { get; init; }

    /// <summary>The specific sub-domain/query within the source module (e.g. "SafetyTrainingReporting").</summary>
    public required string SourceCategory { get; init; }

    /// <summary>Total matching item count returned by the source query, or null when unavailable.</summary>
    public int? ItemCount { get; init; }

    /// <summary>Count of items whose Status indicates active/in-force, or null when unavailable.</summary>
    public int? ActiveItemCount { get; init; }

    /// <summary>Whether this section's live data was retrieved successfully.</summary>
    public required bool IsAvailable { get; init; }

    /// <summary>Sanitized reason this section is unavailable, if applicable.</summary>
    public string? UnavailableReason { get; init; }
}
