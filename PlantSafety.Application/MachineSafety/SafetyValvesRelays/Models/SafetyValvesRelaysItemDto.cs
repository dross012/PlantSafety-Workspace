// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single SafetyValvesRelays record.
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.SafetyValvesRelays.Models;

/// <summary>A single SafetyValvesRelays record (Safety valves and relays ensure machines fail safely during malfunctions via emergency shut-off valves, dual-channel safety relays, and redundant control systems.).</summary>
public sealed record SafetyValvesRelaysItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid SafetyValvesRelaysId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Component Title.</summary>
    public required string ComponentTitle { get; init; }

    /// <summary>Classification: EmergencyShutOffValve, DualChannelSafetyRelay, RedundantControlSystem.</summary>
    public required string ComponentType { get; init; }

    /// <summary>Equipment Location.</summary>
    public required string EquipmentLocation { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string ComponentStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}