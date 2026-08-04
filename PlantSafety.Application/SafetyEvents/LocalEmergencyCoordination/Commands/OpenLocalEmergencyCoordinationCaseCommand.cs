// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           OpenLocalEmergencyCoordinationCaseCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to open a new LocalEmergencyCoordination case. Precedes the six
//                  spec-named workflow commands (EstablishMutualAidAgreement, ActivateJoint
//                  ResponsePlan, AssignExternalResponderLiaison, LogLiaisonCommunication,
//                  RecordJointDebrief, DeactivateJointResponsePlan), which all operate against an
//                  existing case identity -- mirrors DeclareEmergencyCommand's role for
//                  EmergencyManagement. Carries the full command envelope per Handbook Rule 2.1.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;

/// <summary>Opens a new LocalEmergencyCoordination case.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="IncidentId">Optional identity of the SafetyEvents incident (EmergencyManagement/CrisisManagement) this case coordinates external response for.</param>
/// <param name="CaseName">Descriptive name for this coordination case.</param>
/// <param name="OpenedByUserId">Identity of the user opening the case.</param>
public sealed record OpenLocalEmergencyCoordinationCaseCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid? IncidentId,
    string CaseName,
    Guid OpenedByUserId) : ICommand<Result<OpenLocalEmergencyCoordinationCaseResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => OpenedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<OpenLocalEmergencyCoordinationCaseResult>>.CausationId => CausationId;
}

/// <summary>Result of a successful <see cref="OpenLocalEmergencyCoordinationCaseCommand"/> execution.</summary>
/// <param name="LocalEmergencyCoordinationId">Identity of the newly opened LocalEmergencyCoordination case.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record OpenLocalEmergencyCoordinationCaseResult(
    Guid LocalEmergencyCoordinationId,
    string Status);
