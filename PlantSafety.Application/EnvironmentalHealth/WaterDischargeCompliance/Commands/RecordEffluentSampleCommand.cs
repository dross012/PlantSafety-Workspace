// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record a new EffluentSample, or an append-only correction of a prior
//   sample ("EnvironmentalHealth.WaterDischargeCompliance.EffluentSample").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Records a new effluent sample. When <paramref name="CorrectionOfSampleId"/> is set, this
/// creates an append-only correction record; the original sample is never mutated.</summary>
public sealed record RecordEffluentSampleCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid DischargePointId,
    string Parameter,
    decimal MeasuredValue,
    string UnitOfMeasure,
    DateTime SampledAtUtc,
    Guid CollectedByUserId,
    string LabSourceSystem,
    string ChainOfCustodyReference,
    Guid? CorrectionOfSampleId,
    string? CorrectionReason,
    Guid PerformedByUserId) : ICommand<Result<EffluentSampleActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<EffluentSampleActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}