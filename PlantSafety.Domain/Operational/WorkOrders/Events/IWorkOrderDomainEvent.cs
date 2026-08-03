// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           IWorkOrderDomainEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all WorkOrder domain events. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.WorkOrders.Events;

/// <summary>Marker interface for all WorkOrder domain events.</summary>
public interface IWorkOrderDomainEvent : IDomainEvent { }
