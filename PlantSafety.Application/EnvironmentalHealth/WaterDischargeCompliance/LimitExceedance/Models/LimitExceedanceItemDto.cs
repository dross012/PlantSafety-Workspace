// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single LimitExceedance record.
// SchemaVersion: 1

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Models;

/// <summary>A single LimitExceedance read-model record.</summary>
public sealed record LimitExceedanceItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid LimitExceedanceId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the DischargePoint where the exceedance occurred.</summary>
    public required Guid DischargePointId { get; init; }

    /// <summary>Exceeded parameter name.</summary>
    public required string ParameterName { get; init; }

    /// <summary>Measured value.</summary>
    public required decimal MeasuredValue { get; init; }

    /// <summary>Limit value that was exceeded.</summary>
    public required decimal LimitValue { get; init; }

    /// <summary>Exceedance percentage.</summary>
    public required decimal ExceedancePercentage { get; init; }

    /// <summary>Severity classification.</summary>
    public required string Severity { get; init; }

    /// <summary>UTC instant the exceedance was detected.</summary>
    public required DateTime DetectedAtUtc { get; init; }

    /// <summary>Lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}