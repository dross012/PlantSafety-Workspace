// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Domain
//  File:           IEnterpriseFabricDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all EnterpriseFabric domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnterpriseFabric.Events;

/// <summary>Marker interface for all EnterpriseFabric domain events.</summary>
public interface IEnterpriseFabricDomainEvent : IDomainEvent { }
