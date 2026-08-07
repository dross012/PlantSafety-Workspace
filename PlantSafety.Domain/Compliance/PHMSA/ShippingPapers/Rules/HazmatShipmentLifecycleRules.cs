// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           HazmatShipmentLifecycleRules.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Guards for the HazmatShipment aggregate's Draft -> Validated -> Ready ->
//                  Dispatched (or -> Cancelled from any non-terminal status) lifecycle.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Enums;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.HazmatShipment"/> aggregate.</summary>
public static class HazmatShipmentLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a shipping paper may be attached from <paramref name="current"/>.</summary>
    public static bool CanAttachShippingPaper(HazmatShipmentStatus current)
        => current is HazmatShipmentStatus.Draft or HazmatShipmentStatus.Validated;

    /// <summary>Returns <see langword="true"/> when the shipment may be marked ready from <paramref name="current"/>.</summary>
    public static bool CanMarkReady(HazmatShipmentStatus current) => current == HazmatShipmentStatus.Validated;

    /// <summary>Returns <see langword="true"/> when the shipment may be dispatched from <paramref name="current"/>.</summary>
    public static bool CanDispatch(HazmatShipmentStatus current) => current == HazmatShipmentStatus.Ready;

    /// <summary>Returns <see langword="true"/> when the shipment may be cancelled from <paramref name="current"/>.</summary>
    public static bool CanCancel(HazmatShipmentStatus current)
        => current is not (HazmatShipmentStatus.Dispatched or HazmatShipmentStatus.Cancelled);
}
