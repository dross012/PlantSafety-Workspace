// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Application
//  File:           RecordObservationCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to record a new field observation under the SafetyEvents.Observation
//                  bounded context. Carries the full command envelope (CorrelationId, CausationId,
//                  CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.Observation.Commands;

/// <summary>Records a new field observation.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ObservationType">Observation classification: SafeBehavior, AtRiskBehavior, HazardCondition.</param>
/// <param name="HazardCategory">Hazard category, when the observation identifies a hazard.</param>
/// <param name="Location">Physical location the observation was made at.</param>
/// <param name="Description">Narrative description of what was observed.</param>
/// <param name="CorrectiveActionRequired">Indicates whether corrective action is required as a result of this observation.</param>
/// <param name="FollowUpDueAtUtc">UTC timestamp any required follow-up is due.</param>
/// <param name="ObserverUserId">Identity of the worker/observer who captured this observation.</param>
public sealed record RecordObservationCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ObservationType,
    string HazardCategory,
    string Location,
    string Description,
    bool CorrectiveActionRequired,
    DateTime? FollowUpDueAtUtc,
    Guid ObserverUserId) : ICommand<Result<RecordObservationResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ObserverUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RecordObservationResult>>.CausationId => CausationId;
}
