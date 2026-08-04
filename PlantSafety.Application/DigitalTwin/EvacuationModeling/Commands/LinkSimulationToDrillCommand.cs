// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to correlate a completed egress-simulation run with a
//   SafetyEvents.DrillsAndExercises drill (Evacuation Drill Correlation subdomain).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;

/// <summary>Links an <c>EgressSimulationRun</c> to a real evacuation drill for correlation.</summary>
public sealed record LinkSimulationToDrillCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid EgressSimulationRunId,
    Guid DrillId,
    Guid PerformedByUserId) : ICommand<Result<EgressSimulationActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<EgressSimulationActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
