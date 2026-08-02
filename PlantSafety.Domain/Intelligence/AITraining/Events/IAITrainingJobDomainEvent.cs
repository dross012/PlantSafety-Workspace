// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Domain
//  File:           IAITrainingJobDomainEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Intelligence.AITraining domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.Intelligence.AITraining.Events;

/// <summary>Marker interface for all Intelligence.AITraining domain events.</summary>
public interface IAITrainingJobDomainEvent : IDomainEvent { }
