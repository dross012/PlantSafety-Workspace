// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Read-model item DTO for a single SafetyProgram list row. Field names
//   mirror PlantSafety.UI.Modules.SafetyPrograms.Models.SafetyProgramListItemDto exactly so
//   the API response deserializes cleanly into the existing UI client DTO.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyPrograms.Models;

/// <summary>A single SafetyProgram list-view read-model row.</summary>
public sealed record SafetyProgramListItemDto
{
    /// <summary>SafetyProgram identifier.</summary>
    public required Guid ProgramId { get; init; }

    /// <summary>Program name.</summary>
    public required string Name { get; init; }

    /// <summary>Program category (Training/Behavioral/Compliance/Recognition).</summary>
    public required string Category { get; init; }

    /// <summary>Current lifecycle status (Planned/Active/OnHold/Completed).</summary>
    public required string Status { get; init; }

    /// <summary>Participation percentage (0-100).</summary>
    public required double ParticipationPercent { get; init; }

    /// <summary>Owner display name.</summary>
    public required string OwnerName { get; init; }
}
