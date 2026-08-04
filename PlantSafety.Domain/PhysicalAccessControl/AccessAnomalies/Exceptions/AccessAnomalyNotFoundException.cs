// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomalyNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AccessAnomaly aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Exceptions;

/// <summary>Thrown when an <see cref="AccessAnomaly"/> aggregate lookup finds no match.</summary>
public sealed class AccessAnomalyNotFoundException : AccessAnomalyDomainException
{
    /// <summary>Creates a not-found exception for the given AccessAnomaly identity.</summary>
    public AccessAnomalyNotFoundException(AccessAnomalyId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"AccessAnomaly with id '{id}' was not found.",
            "AccessAnomaly.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
