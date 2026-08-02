// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Application
//  File:           DeclareEmergencyCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to declare a new emergency under the SafetyEvents.EmergencyManagement
//                  bounded context. Carries the full command envelope (CorrelationId, CausationId,
//                  CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Commands;

/// <summary>Declares a new emergency.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="EmergencyType">Emergency classification (e.g., Fire, ChemicalRelease, MedicalEmergency, Evacuation, SevereWeather).</param>
/// <param name="Severity">Emergency severity: Minor, Moderate, Major, Catastrophic.</param>
/// <param name="Location">Physical location the emergency was declared at.</param>
/// <param name="CasualtyCount">Count of casualties/injuries associated with this emergency.</param>
/// <param name="ResponseTeamNotified">Indicates whether the emergency response team has already been notified at declaration time.</param>
/// <param name="DeclaredByUserId">Identity of the user declaring the emergency.</param>
public sealed record DeclareEmergencyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string EmergencyType,
    string Severity,
    string Location,
    int CasualtyCount,
    bool ResponseTeamNotified,
    Guid DeclaredByUserId) : ICommand<Result<DeclareEmergencyResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => DeclaredByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<DeclareEmergencyResult>>.CausationId => CausationId;
}
