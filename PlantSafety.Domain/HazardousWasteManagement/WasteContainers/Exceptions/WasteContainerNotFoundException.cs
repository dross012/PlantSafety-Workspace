// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           WasteContainerNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a WasteContainer aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Exceptions;

/// <summary>Thrown when a WasteContainer aggregate lookup finds no match.</summary>
public sealed class WasteContainerNotFoundException : WasteContainerDomainException
{
    /// <summary>Creates a not-found exception for the given WasteContainer identity.</summary>
    public WasteContainerNotFoundException(WasteContainerId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"WasteContainer with id '{id}' was not found.",
            "WasteContainer.NotFound", "NotFound", "Medium", correlationId, causationId)
    {
    }
}
