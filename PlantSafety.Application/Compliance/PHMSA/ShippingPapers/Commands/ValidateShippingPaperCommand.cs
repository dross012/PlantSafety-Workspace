// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           ValidateShippingPaperCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Re-invokes ShippingPaperGenerator against the shipment's current materials and
//                  compares the result to what is currently persisted, returning discrepancies.
//                  Mirrors ValidatePlacardingForShipmentCommand's approach in the sibling
//                  Placarding module. Side-effect-free; performs no persistence.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Commands;

/// <summary>Re-evaluates the shipping paper for an existing HazmatShipment and compares it to what is currently persisted.</summary>
public sealed record ValidateShippingPaperCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid HazmatShipmentId,
    Guid RequestedByUserId) : ICommand<Result<ValidateShippingPaperResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ValidateShippingPaperResult>>.CausationId => CausationId;
}

/// <summary>Result of <see cref="ValidateShippingPaperCommand"/>.</summary>
/// <param name="HazmatShipmentId">Identity of the validated HazmatShipment.</param>
/// <param name="IsConsistent">Whether the persisted shipping paper matches the freshly computed one.</param>
/// <param name="PersistedBasicDescription">Basic description sequence currently persisted on the shipment.</param>
/// <param name="ComputedBasicDescription">Basic description sequence freshly computed from the shipment's current materials.</param>
/// <param name="Discrepancies">Human-readable discrepancy messages, empty when consistent.</param>
public sealed record ValidateShippingPaperResult(
    Guid HazmatShipmentId,
    bool IsConsistent,
    string? PersistedBasicDescription,
    string? ComputedBasicDescription,
    IReadOnlyList<string> Discrepancies);
