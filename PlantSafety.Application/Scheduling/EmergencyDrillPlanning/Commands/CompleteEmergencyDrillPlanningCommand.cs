// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Application
//  File:           CompleteEmergencyDrillPlanningCommand.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Command to record the post-drill debrief/completion (Active -> Completed).
//                  Implements IConcurrencyAwareCommand from the start. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CompletePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Commands;

/// <summary>Records the post-drill debrief/completion (Active -&gt; Completed).</summary>
/// <param name="DebriefNotes">Optional post-drill debrief notes.</param>
public sealed record CompleteEmergencyDrillPlanningCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid EmergencyDrillPlanningId,
    Guid PerformedByUserId,
    string? DebriefNotes) : ICommand<Result<EmergencyDrillPlanningStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<EmergencyDrillPlanningStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
