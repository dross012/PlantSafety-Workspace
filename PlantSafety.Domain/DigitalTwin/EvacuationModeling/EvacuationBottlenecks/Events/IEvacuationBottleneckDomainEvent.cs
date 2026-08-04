// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           IEvacuationBottleneckDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all EvacuationBottleneck domain events. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Events;

/// <summary>Marker interface for all EvacuationBottleneck domain events.</summary>
public interface IEvacuationBottleneckDomainEvent : IDomainEvent { }
