// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single Equipment & Machine Operation Policies record.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyPolicies.EquipmentMachineOperation.Models;

/// <summary>A single Equipment & Machine Operation Policies read-model item.</summary>
public sealed record EquipmentMachineOperationItemDto
{
    /// <summary>SafetyPolicy identifier.</summary>
    public required Guid SafetyPolicyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Title of the underlying safetypolicy record.</summary>
    public required string Title { get; init; }

    /// <summary>PolicyCategory code this item is classified under.</summary>
    public required string Category { get; init; }

    /// <summary>Current lifecycle status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp the record became effective or was recorded.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>Identity of the accountable owner.</summary>
    public required Guid OwnerId { get; init; }
}