// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           ITransportTripDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all TransportTrip domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Events;

/// <summary>Marker interface for all TransportTrip domain events.</summary>
public interface ITransportTripDomainEvent : IDomainEvent { }
