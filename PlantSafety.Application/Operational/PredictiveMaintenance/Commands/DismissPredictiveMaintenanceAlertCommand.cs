// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           DismissPredictiveMaintenanceAlertCommand.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Command to dismiss a PredictiveMaintenanceAlert without corrective action
//                  (e.g. false positive) (Raised/UnderReview -> Dismissed). Implements
//                  IConcurrencyAwareCommand from the start. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.RejectPermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Commands;

/// <summary>Dismisses a PredictiveMaintenanceAlert without corrective action, recording the reason.</summary>
public sealed record DismissPredictiveMaintenanceAlertCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PredictiveMaintenanceAlertId,
    Guid DismissedByUserId,
    string Reason) : ICommand<Result<PredictiveMaintenanceAlertStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => DismissedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PredictiveMaintenanceAlertStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
