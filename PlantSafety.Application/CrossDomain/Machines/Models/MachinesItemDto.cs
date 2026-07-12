// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Machine record.
// SchemaVersion: 1

namespace PlantSafety.Application.CrossDomain.Machines.Models;

/// <summary>A single machine/equipment record.</summary>
public sealed record MachinesItemDto
{
    /// <summary>Machine identifier.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Machine display name.</summary>
    public required string MachineName { get; init; }

    /// <summary>Machine type/class (e.g., Press/Conveyor/CNC).</summary>
    public required string MachineType { get; init; }

    /// <summary>Manufacturer serial number.</summary>
    public required string SerialNumber { get; init; }

    /// <summary>Current machine status (e.g., Operational/Maintenance/OutOfService).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp of the machine's last service.</summary>
    public required DateTime LastServicedAtUtc { get; init; }

    /// <summary>Identity of the technician who last serviced the machine.</summary>
    public required Guid LastServicedBy { get; init; }
}
