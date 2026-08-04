// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           ILocalEmergencyCoordinationDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all LocalEmergencyCoordination domain events. Also serves
//                  as the event-based integration point for a future PublicSafetyCommunication
//                  module: any external consumer (including one that does not yet exist in this
//                  repository) can subscribe to events implementing this interface via the outbox
//                  without LocalEmergencyCoordination taking a compile-time dependency on it.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Events;

/// <summary>Marker interface for all LocalEmergencyCoordination domain events.</summary>
public interface ILocalEmergencyCoordinationDomainEvent : IDomainEvent { }
