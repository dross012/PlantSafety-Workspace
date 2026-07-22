// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures
//  Layer:          Domain
//  File:           ISafetyProceduresDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level SafetyProcedures domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.Events;

/// <summary>Marker interface for top-level SafetyProcedures domain events.</summary>
public interface ISafetyProceduresDomainEvent : IDomainEvent { }