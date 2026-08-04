// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single DischargeLimit record.
// SchemaVersion: 1

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Models;

/// <summary>A single DischargeLimit read-model record.</summary>
public sealed record DischargeLimitItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid DischargeLimitId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the DischargePoint this limit governs.</summary>
    public required Guid DischargePointId { get; init; }

    /// <summary>Parameter this limit governs.</summary>
    public required string Parameter { get; init; }

    /// <summary>Statistical basis.</summary>
    public required string LimitBasis { get; init; }

    /// <summary>Current limit value.</summary>
    public required decimal LimitValue { get; init; }

    /// <summary>Unit of measure.</summary>
    public required string UnitOfMeasure { get; init; }

    /// <summary>Permit condition reference (NPDES condition mapping).</summary>
    public required string PermitConditionReference { get; init; }

    /// <summary>Lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}