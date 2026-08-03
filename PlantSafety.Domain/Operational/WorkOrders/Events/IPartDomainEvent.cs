// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           IPartDomainEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Part (inventory) domain events. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.WorkOrders.Events;

/// <summary>Marker interface for all Part domain events.</summary>
public interface IPartDomainEvent : IDomainEvent { }
