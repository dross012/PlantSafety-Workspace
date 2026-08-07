// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           TransportMode.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Mode of transport for a hazmat shipment, driving modal-specific shipping-paper
//                  requirements (see Services.ShippingPaperGenerator).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Enums;

/// <summary>Mode of transport for a <see cref="Aggregates.HazmatShipment"/>.</summary>
public enum TransportMode
{
    /// <summary>Highway/motor carrier transport.</summary>
    Ground = 0,

    /// <summary>Air transport.</summary>
    Air = 1,

    /// <summary>Vessel/marine transport.</summary>
    Vessel = 2,

    /// <summary>Rail transport.</summary>
    Rail = 3,
}
