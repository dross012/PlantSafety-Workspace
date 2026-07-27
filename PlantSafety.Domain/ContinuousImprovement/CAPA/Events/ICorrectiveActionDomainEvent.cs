// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Domain
//  File:           ICorrectiveActionDomainEvent.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all CAPA CorrectiveAction domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.ContinuousImprovement.CAPA.Events;

/// <summary>Marker interface for all CAPA CorrectiveAction domain events.</summary>
public interface ICorrectiveActionDomainEvent : IDomainEvent { }
