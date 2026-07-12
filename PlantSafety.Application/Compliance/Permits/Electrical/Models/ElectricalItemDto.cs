// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single electrical work permit record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Permits.Electrical.Models;

/// <summary>A single electrical work permit record.</summary>
public sealed record ElectricalItemDto
{
    /// <summary>Permit identifier.</summary>
    public required Guid PermitId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the equipment being worked on.</summary>
    public required string EquipmentIdentifier { get; init; }

    /// <summary>Voltage classification of the work (e.g., LowVoltage, HighVoltage).</summary>
    public required string VoltageClass { get; init; }

    /// <summary>Current permit status (e.g., Active/Expired/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the permit was issued.</summary>
    public required DateTime IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp when the permit expires.</summary>
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>Identity of the user who approved the permit.</summary>
    public required Guid ApprovedBy { get; init; }
}
