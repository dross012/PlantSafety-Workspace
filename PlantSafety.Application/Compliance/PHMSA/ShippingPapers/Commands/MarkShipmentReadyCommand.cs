// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           MarkShipmentReadyCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to mark a HazmatShipment ready for dispatch (Validated -> Ready).
//                  Requires a shipping paper already attached and zero Prohibited-severity
//                  segregation violations -- enforced by the aggregate itself. Implements
//                  IConcurrencyAwareCommand. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Commands;

/// <summary>Marks a HazmatShipment ready for dispatch (Validated -&gt; Ready).</summary>
/// <param name="ErgGuideNumber">Emergency Response Guidebook guide number.</param>
/// <param name="EmergencyContactName">Emergency contact name.</param>
/// <param name="EmergencyContactPhone">Emergency contact phone.</param>
/// <param name="ResponseGuidanceSummary">Emergency response guidance summary.</param>
/// <param name="CommandId">Unique identifier for this command instance.</param>
/// <param name="CorrelationId">Correlation identifier grouping every operation in the same business transaction.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event that caused this one.</param>
/// <param name="TenantId">Tenant the operation is scoped to.</param>
/// <param name="HazmatShipmentId">Identifier of the HazmatShipment record.</param>
/// <param name="PerformedByUserId">Identifier of the user performing this operation.</param>
public sealed record MarkShipmentReadyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid HazmatShipmentId,
    string ErgGuideNumber,
    string EmergencyContactName,
    string EmergencyContactPhone,
    string ResponseGuidanceSummary,
    Guid PerformedByUserId) : ICommand<Result<HazmatShipmentStatusResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<HazmatShipmentStatusResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}
