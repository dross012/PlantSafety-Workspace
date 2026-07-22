// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single MachineProcessMonitoring record. Machine & process safety monitoring: LOTO state, robotic cell activity, unexpected startups, e-stop activation, bypassed guards.
// SchemaVersion: 1

namespace PlantSafety.Application.Observability.MachineProcessMonitoring.Models;

/// <summary>A single MachineProcessMonitoring read-model record.</summary>
public sealed record MachineProcessMonitoringItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid MachineProcessMonitoringId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>MachineId.</summary>
    public required  Guid MachineId { get; init; }

    /// <summary>SensorType.</summary>
    public required  string SensorType { get; init; }

    /// <summary>Reading.</summary>
    public required  decimal Reading { get; init; }

    /// <summary>ThresholdBreached.</summary>
    public required  bool ThresholdBreached { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}