// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Domain
//  File:           IPredictiveSafetyAlertDomainEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Intelligence.PredictiveSafety domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.Intelligence.PredictiveSafety.Events;

/// <summary>Marker interface for all Intelligence.PredictiveSafety domain events.</summary>
public interface IPredictiveSafetyAlertDomainEvent : IDomainEvent { }
