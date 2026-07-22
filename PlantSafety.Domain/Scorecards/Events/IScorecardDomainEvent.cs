// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           IScorecardDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Scorecard domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.Scorecards.Events;

/// <summary>Marker interface for all Scorecard domain events.</summary>
public interface IScorecardDomainEvent : IDomainEvent { }