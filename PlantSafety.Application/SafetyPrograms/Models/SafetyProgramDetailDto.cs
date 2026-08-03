// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Full read-model DTO for a single SafetyProgram, returned by
//   GetSafetyProgramDetailQuery. Extends the original UI-only DTO shape with a
//   LinkedActivity rollup (additive field) composed live at query time from the sibling
//   SafetyPolicies/SafetyProcedures/SafetyCulture modules -- see
//   GetSafetyProgramDetailQueryHandler for the composition logic.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyPrograms.Models;

/// <summary>Full SafetyProgram detail read-model.</summary>
public sealed record SafetyProgramDetailDto
{
    /// <summary>SafetyProgram identifier.</summary>
    public required Guid ProgramId { get; init; }

    /// <summary>Program name.</summary>
    public required string Name { get; init; }

    /// <summary>Program description.</summary>
    public required string Description { get; init; }

    /// <summary>Program category.</summary>
    public required string Category { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>Program start date.</summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>Program end date, if any.</summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>Participation percentage (0-100).</summary>
    public required double ParticipationPercent { get; init; }

    /// <summary>Enrolled participant count.</summary>
    public required int EnrolledCount { get; init; }

    /// <summary>Owner display name.</summary>
    public required string OwnerName { get; init; }

    /// <summary>Program goals/objectives.</summary>
    public IReadOnlyList<string> Goals { get; init; } = [];

    /// <summary>
    /// Live rollup of related activity in the sibling SafetyPolicies/SafetyProcedures/
    /// SafetyCulture modules for this program's Category, composed at query time (never
    /// snapshotted). Additive field -- absent/empty for programs whose Category has no
    /// curated composition mapping.
    /// </summary>
    public IReadOnlyList<SafetyProgramLinkedActivityDto> LinkedActivity { get; init; } = [];
}
