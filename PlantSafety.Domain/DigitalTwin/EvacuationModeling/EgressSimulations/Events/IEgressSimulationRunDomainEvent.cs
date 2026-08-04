// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           IEgressSimulationRunDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all EgressSimulationRun domain events. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Events;

/// <summary>Marker interface for all EgressSimulationRun domain events.</summary>
public interface IEgressSimulationRunDomainEvent : IDomainEvent { }
