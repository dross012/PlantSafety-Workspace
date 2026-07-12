// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single WorkInstructions record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Procedures.WorkInstructions.Models;

/// <summary>A single WorkInstructions read-model item.</summary>
public sealed record WorkInstructionsItemDto
{
    /// <summary>Instruction Id identifier.</summary>
    public required Guid InstructionId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Title.</summary>
    public required string Title { get; init; }

    /// <summary>Work Area.</summary>
    public required string WorkArea { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Effective At Utc.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>Owner Id.</summary>
    public required Guid OwnerId { get; init; }
}
