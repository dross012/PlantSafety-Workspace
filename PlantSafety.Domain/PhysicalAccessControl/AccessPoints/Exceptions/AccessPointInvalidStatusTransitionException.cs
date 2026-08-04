// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           AccessPointInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AccessPoint lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Exceptions;

/// <summary>Thrown when an <see cref="AccessPoint"/> lifecycle transition is not permitted.</summary>
public sealed class AccessPointInvalidStatusTransitionException : AccessPointDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AccessPointInvalidStatusTransitionException(
        AccessPointId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'AccessPoint' {id} from '{from}' to '{to}'.",
            "AccessPoint.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
