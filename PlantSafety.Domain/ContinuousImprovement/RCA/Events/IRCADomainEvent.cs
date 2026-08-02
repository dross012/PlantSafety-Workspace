// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Domain
//  File:           IRCADomainEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all ContinuousImprovement RCA domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.ContinuousImprovement.RCA.Events;

/// <summary>Marker interface for all ContinuousImprovement RCA domain events.</summary>
public interface IRCADomainEvent : IDomainEvent { }
