// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           AdjustPartStockCommand.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Command to adjust a Part's on-hand quantity (restock or consumption).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.WorkOrders.Commands;

/// <summary>Adjusts a Part's on-hand quantity.</summary>
public sealed record AdjustPartStockCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PartId,
    int DeltaQuantity,
    string? Reason,
    Guid RequestedByUserId) : ICommand<Result<PartStockResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PartStockResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}

/// <summary>Result of a successful <see cref="AdjustPartStockCommand"/> execution.</summary>
/// <param name="PartId">Identity of the adjusted Part aggregate.</param>
/// <param name="QuantityOnHand">Quantity on hand after the adjustment.</param>
public sealed record PartStockResult(Guid PartId, int QuantityOnHand);
