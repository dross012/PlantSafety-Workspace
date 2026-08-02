// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Application
//  File:           CreateNearMissesCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to report a new NearMisses record under the SafetyEvents.NearMisses
//                  bounded context. Carries the full command envelope (CorrelationId, CausationId,
//                  CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Operational.PredictiveMaintenance.Commands
//                  .RaisePredictiveMaintenanceAlertCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.NearMisses.Commands;

/// <summary>Reports a new NearMisses record.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="Category">Hazard category (e.g., Slip/Trip/Fall, EquipmentFailure, ChemicalExposure).</param>
/// <param name="PotentialSeverity">Potential severity had the event resulted in an actual incident.</param>
/// <param name="ReportedAtUtc">UTC instant the near-miss was reported.</param>
/// <param name="ReportedByUserId">Identity of the user reporting the near-miss.</param>
public sealed record CreateNearMissesCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string Category,
    string PotentialSeverity,
    DateTime ReportedAtUtc,
    Guid ReportedByUserId) : ICommand<Result<NearMissesStatusResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ReportedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<NearMissesStatusResult>>.CausationId => CausationId;
}
