// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a WasteStream lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Exceptions;

/// <summary>Thrown when a WasteStream lifecycle transition is not permitted.</summary>
public sealed class WasteStreamInvalidStatusTransitionException : WasteStreamDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public WasteStreamInvalidStatusTransitionException(
        WasteStreamId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'WasteStream' {id} from '{from}' to '{to}'.",
            "WasteStream.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
