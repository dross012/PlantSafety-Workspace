// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Domain
//  File:           DigitalTwinInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a DigitalTwin lifecycle transition is not permitted. Phase 4 full
//                  build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.Exceptions;

/// <summary>Thrown when a DigitalTwin lifecycle transition is not permitted.</summary>
public sealed class DigitalTwinInvalidStatusTransitionException : DigitalTwinDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public DigitalTwinInvalidStatusTransitionException(
        Guid id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition DigitalTwin {id} from '{from}' to '{to}'.",
            "DigitalTwin.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
