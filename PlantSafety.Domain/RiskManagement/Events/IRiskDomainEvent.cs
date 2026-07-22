// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           IRiskDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Risk domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.RiskManagement.Events;

/// <summary>Marker interface for all Risk domain events.</summary>
public interface IRiskDomainEvent : IDomainEvent { }