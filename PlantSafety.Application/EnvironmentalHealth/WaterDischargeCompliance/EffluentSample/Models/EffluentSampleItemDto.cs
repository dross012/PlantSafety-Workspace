// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single EffluentSample record.
// SchemaVersion: 1

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Models;

/// <summary>A single EffluentSample read-model record.</summary>
public sealed record EffluentSampleItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EffluentSampleId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the DischargePoint this sample was collected from.</summary>
    public required Guid DischargePointId { get; init; }

    /// <summary>Parameter measured.</summary>
    public required string Parameter { get; init; }

    /// <summary>Measured value.</summary>
    public required decimal MeasuredValue { get; init; }

    /// <summary>Unit of measure.</summary>
    public required string UnitOfMeasure { get; init; }

    /// <summary>UTC instant the sample was collected.</summary>
    public required DateTime SampledAtUtc { get; init; }

    /// <summary>Lab source system tag (data-governance requirement).</summary>
    public required string LabSourceSystem { get; init; }

    /// <summary>Chain-of-custody reference.</summary>
    public required string ChainOfCustodyReference { get; init; }

    /// <summary>Identity of the original sample this record corrects, if any.</summary>
    public Guid? CorrectionOfSampleId { get; init; }

    /// <summary>Lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}