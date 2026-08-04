// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single DischargePoint record.
// SchemaVersion: 1

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Models;

/// <summary>A single DischargePoint read-model record.</summary>
public sealed record DischargePointItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid DischargePointId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the owning DischargePermit.</summary>
    public required Guid DischargePermitId { get; init; }

    /// <summary>Discharge point code.</summary>
    public required string PointCode { get; init; }

    /// <summary>Discharge point display name.</summary>
    public required string PointName { get; init; }

    /// <summary>Receiving water body.</summary>
    public required string ReceivingWaterBody { get; init; }

    /// <summary>Outfall classification.</summary>
    public required string Outfall { get; init; }

    /// <summary>Lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}