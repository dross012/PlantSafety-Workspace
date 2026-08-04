// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a CraneHoistSystems lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Exceptions;

/// <summary>Thrown when a <see cref="CraneHoistSystems"/> lifecycle transition is not permitted.</summary>
public sealed class CraneHoistSystemsInvalidStatusTransitionException : CraneHoistSystemsDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CraneHoistSystemsInvalidStatusTransitionException(
        CraneHoistSystemsId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'CraneHoistSystems' {id} from '{from}' to '{to}'.",
            "CraneHoistSystems.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
