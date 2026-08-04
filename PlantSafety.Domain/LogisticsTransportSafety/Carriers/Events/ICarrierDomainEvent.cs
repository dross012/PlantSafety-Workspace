// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           ICarrierDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Carrier domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Events;

/// <summary>Marker interface for all Carrier domain events.</summary>
public interface ICarrierDomainEvent : IDomainEvent { }
