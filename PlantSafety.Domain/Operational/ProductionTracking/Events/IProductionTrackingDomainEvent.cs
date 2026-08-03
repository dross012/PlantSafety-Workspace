// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           IProductionTrackingDomainEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all ProductionTracking domain events (ProductionLine and
//                  ProductionRun aggregates share the same event marker since they are two
//                  aggregate roots within the same small bounded context). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.ProductionTracking.Events;

/// <summary>Marker interface for all ProductionTracking domain events.</summary>
public interface IProductionTrackingDomainEvent : IDomainEvent { }
