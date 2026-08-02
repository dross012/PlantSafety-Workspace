// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Domain
//  File:           EnterpriseFabricInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when an EnterpriseFabric lifecycle transition is not permitted. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Exceptions
//                  .AuditingInvalidStatusTransitionException.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnterpriseFabric.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.EnterpriseFabric"/> lifecycle transition is not permitted.</summary>
public sealed class EnterpriseFabricInvalidStatusTransitionException : EnterpriseFabricDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public EnterpriseFabricInvalidStatusTransitionException(
        EnterpriseFabricId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'EnterpriseFabric' {id} from '{from}' to '{to}'.",
            "EnterpriseFabric.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
