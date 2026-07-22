// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           ISafetyPolicyDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all SafetyPolicy domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.SafetyPolicies.Events;

/// <summary>Marker interface for all SafetyPolicy domain events.</summary>
public interface ISafetyPolicyDomainEvent : IDomainEvent { }