// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           RaisePredictiveMaintenanceAlertCommand.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Command to raise a new PredictiveMaintenanceAlert under the
//                  Operational.PredictiveMaintenance bounded context. Carries the full command
//                  envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Commands;

/// <summary>Raises a new PredictiveMaintenanceAlert.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="EquipmentId">Identifier of the equipment being monitored.</param>
/// <param name="PredictedFailureType">Predicted failure mode/type.</param>
/// <param name="PredictedAtUtc">UTC instant the failure was predicted.</param>
/// <param name="RaisedByUserId">Identity of the user or system actor raising the alert.</param>
public sealed record RaisePredictiveMaintenanceAlertCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid EquipmentId,
    string PredictedFailureType,
    DateTime PredictedAtUtc,
    Guid RaisedByUserId) : ICommand<Result<PredictiveMaintenanceAlertStatusResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RaisedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PredictiveMaintenanceAlertStatusResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="RaisePredictiveMaintenanceAlertCommand"/> from raw, untrusted transport values.</summary>
    public static RaisePredictiveMaintenanceAlertCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid equipmentId,
        string predictedFailureType,
        DateTime predictedAtUtc,
        Guid raisedByUserId,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            EquipmentId: equipmentId,
            PredictedFailureType: (predictedFailureType ?? string.Empty).Trim(),
            PredictedAtUtc: predictedAtUtc,
            RaisedByUserId: raisedByUserId);
}
