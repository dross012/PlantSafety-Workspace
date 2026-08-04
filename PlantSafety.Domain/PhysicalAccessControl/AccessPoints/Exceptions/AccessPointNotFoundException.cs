// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           AccessPointNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AccessPoint aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Exceptions;

/// <summary>Thrown when an <see cref="AccessPoint"/> aggregate lookup finds no match.</summary>
public sealed class AccessPointNotFoundException : AccessPointDomainException
{
    /// <summary>Creates a not-found exception for the given AccessPoint identity.</summary>
    public AccessPointNotFoundException(AccessPointId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"AccessPoint with id '{id}' was not found.",
            "AccessPoint.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
