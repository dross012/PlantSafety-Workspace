// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           PPEZoneMappingInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PPEZoneMapping lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.Exceptions;

/// <summary>Thrown when a <see cref="PPEZoneMapping"/> lifecycle transition is not permitted.</summary>
public sealed class PPEZoneMappingInvalidStatusTransitionException : PPEZoneMappingDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PPEZoneMappingInvalidStatusTransitionException(
        PPEZoneMappingId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'PPEZoneMapping' {id} from '{from}' to '{to}'.",
            "PPEZoneMapping.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}