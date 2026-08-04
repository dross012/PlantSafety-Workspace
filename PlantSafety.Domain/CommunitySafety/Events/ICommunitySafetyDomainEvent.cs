// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety
//  Layer:          Domain
//  File:           ICommunitySafetyDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level CommunitySafety domain events. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.Events;

/// <summary>Marker interface for top-level CommunitySafety domain events.</summary>
public interface ICommunitySafetyDomainEvent : IDomainEvent { }
