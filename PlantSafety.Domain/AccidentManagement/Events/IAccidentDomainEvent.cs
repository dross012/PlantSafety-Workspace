// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           IAccidentDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Accident domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.AccidentManagement.Events;

/// <summary>Marker interface for all Accident domain events.</summary>
public interface IAccidentDomainEvent : IDomainEvent { }