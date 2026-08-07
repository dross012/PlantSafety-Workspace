// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           HazmatShipmentStatus.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the HazmatShipment aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.HazmatShipment"/> aggregate root.</summary>
public enum HazmatShipmentStatus
{
    /// <summary>Shipment record created; not yet validated.</summary>
    Draft = 0,

    /// <summary>Shipping paper has been attached and the shipment is validated.</summary>
    Validated = 1,

    /// <summary>Shipment is ready for dispatch (emergency response info attached, no prohibited segregation violations).</summary>
    Ready = 2,

    /// <summary>Shipment has been dispatched.</summary>
    Dispatched = 3,

    /// <summary>Shipment has been cancelled.</summary>
    Cancelled = 4,
}
