// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           WasteContainerInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a WasteContainer lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Exceptions;

/// <summary>Thrown when a WasteContainer lifecycle transition is not permitted.</summary>
public sealed class WasteContainerInvalidStatusTransitionException : WasteContainerDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public WasteContainerInvalidStatusTransitionException(
        WasteContainerId id, string from, string to, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'WasteContainer' {id} from '{from}' to '{to}'.",
            "WasteContainer.InvalidTransition", "Lifecycle", "Medium", correlationId, causationId)
    {
    }
}
