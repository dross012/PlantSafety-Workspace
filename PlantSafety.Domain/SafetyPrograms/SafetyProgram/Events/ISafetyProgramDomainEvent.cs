// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           ISafetyProgramDomainEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all SafetyProgram domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.Events;

/// <summary>Marker interface for all SafetyProgram domain events.</summary>
public interface ISafetyProgramDomainEvent : IDomainEvent { }
