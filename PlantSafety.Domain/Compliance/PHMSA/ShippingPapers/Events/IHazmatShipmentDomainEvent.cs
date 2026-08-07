// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           IHazmatShipmentDomainEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all HazmatShipment domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Events;

/// <summary>Marker interface for all HazmatShipment domain events.</summary>
public interface IHazmatShipmentDomainEvent : IDomainEvent { }
