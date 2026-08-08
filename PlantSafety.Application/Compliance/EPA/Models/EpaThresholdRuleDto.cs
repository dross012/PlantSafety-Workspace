// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.EpaThresholdRule row -- a
//   numeric regulatory threshold (quantity, time limit, reportable quantity) defined by an
//   EPA program, optionally tied to a single EpaRegulatoryRequirement. Global reference data,
//   not tenant-scoped.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.EPA.Models;

/// <summary>Flat read-model DTO for a single Compliance.EpaThresholdRule row.</summary>
public sealed record EpaThresholdRuleDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid ThresholdRuleId { get; init; }

    /// <summary>The EPA program this threshold belongs to (e.g. "RCRA").</summary>
    public required string ProgramCode { get; init; }

    /// <summary>Optional link back to the requirement this threshold operationalizes.</summary>
    public Guid? RequirementId { get; init; }

    /// <summary>Name of the parameter being thresholded (e.g. "Hazardous waste generation rate").</summary>
    public required string ParameterName { get; init; }

    /// <summary>Numeric limit value.</summary>
    public required decimal LimitValue { get; init; }

    /// <summary>Unit of measure for <see cref="LimitValue"/> (e.g. "kg/month").</summary>
    public required string Unit { get; init; }

    /// <summary>Free-text applicability condition (e.g. generator status).</summary>
    public string? ConditionExpression { get; init; }

    /// <summary>When this row was created.</summary>
    public required DateTime CreatedOn { get; init; }
}
