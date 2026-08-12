// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           ValidatePlacardingForShipmentCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Loads a HazmatShipment (via its Domain repository) and re-runs
//                  PlacardingEngine against its known materials/transport mode, comparing the
//                  freshly-computed placard set to the shipment's persisted
//                  PlacardingRequirements. Side-effect-free; performs no persistence.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>
/// Re-evaluates placarding for an existing HazmatShipment and compares the computed result to
/// what is currently persisted on the shipment.
/// </summary>
/// <param name="ShipmentId">Identity of the HazmatShipment to validate.</param>
/// <param name="IsBulkPackaging">Whether the shipment's load is carried in bulk packaging.</param>
/// <param name="CommandId">Unique identifier for this command instance.</param>
/// <param name="CorrelationId">Correlation identifier grouping every operation in the same business transaction.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event that caused this one.</param>
/// <param name="TenantId">Tenant the operation is scoped to.</param>
/// <param name="RequestedByUserId">Identifier of the user performing this operation.</param>
public sealed record ValidatePlacardingForShipmentCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid ShipmentId,
    bool IsBulkPackaging,
    Guid RequestedByUserId) : ICommand<Result<ValidatePlacardingForShipmentResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ValidatePlacardingForShipmentResult>>.CausationId => CausationId;
}

/// <summary>Result of <see cref="ValidatePlacardingForShipmentCommand"/>.</summary>
/// <param name="ShipmentId">Identity of the validated HazmatShipment.</param>
/// <param name="IsConsistent">Whether the persisted PlacardingRequirements match the freshly computed set.</param>
/// <param name="PersistedPlacards">Placards currently persisted on the shipment.</param>
/// <param name="ComputedPlacards">Placards freshly computed from the shipment's current materials.</param>
/// <param name="Discrepancies">Human-readable discrepancy messages, empty when consistent.</param>
public sealed record ValidatePlacardingForShipmentResult(
    Guid ShipmentId,
    bool IsConsistent,
    IReadOnlyList<string> PersistedPlacards,
    IReadOnlyList<string> ComputedPlacards,
    IReadOnlyList<string> Discrepancies);
