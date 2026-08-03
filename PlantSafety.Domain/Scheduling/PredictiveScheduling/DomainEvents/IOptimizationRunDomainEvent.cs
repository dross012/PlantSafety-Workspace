// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           IOptimizationRunDomainEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all OptimizationRun domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.DomainEvents;

/// <summary>Marker interface for all OptimizationRun domain events.</summary>
public interface IOptimizationRunDomainEvent : IDomainEvent { }
