// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a WasteStream aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Exceptions;

/// <summary>Thrown when a WasteStream aggregate lookup finds no match.</summary>
public sealed class WasteStreamNotFoundException : WasteStreamDomainException
{
    /// <summary>Creates a not-found exception for the given WasteStream identity.</summary>
    public WasteStreamNotFoundException(WasteStreamId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"WasteStream with id '{id}' was not found.",
            "WasteStream.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
