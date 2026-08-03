// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionRunInvalidStatusTransitionException.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ProductionRun lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.ProductionTracking.Exceptions;

/// <summary>Thrown when a ProductionRun lifecycle transition is not permitted.</summary>
public sealed class ProductionRunInvalidStatusTransitionException : ProductionTrackingDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ProductionRunInvalidStatusTransitionException(
        ProductionRunId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition ProductionRun '{id}' from '{from}' to '{to}'.",
            "ProductionRun.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
