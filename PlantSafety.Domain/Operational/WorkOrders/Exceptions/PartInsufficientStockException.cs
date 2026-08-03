// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           PartInsufficientStockException.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Thrown when a stock adjustment would drive a Part's QuantityOnHand negative.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.WorkOrders.Exceptions;

/// <summary>Thrown when a stock adjustment would drive a Part's QuantityOnHand negative.</summary>
public sealed class PartInsufficientStockException : PartDomainException
{
    /// <summary>Creates an insufficient-stock exception for the given part and requested delta.</summary>
    public PartInsufficientStockException(
        PartId id, int currentQuantity, int requestedDelta,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Part '{id}' has insufficient stock ({currentQuantity}) to apply adjustment of {requestedDelta}.",
            "Part.InsufficientStock",
            "Inventory",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
