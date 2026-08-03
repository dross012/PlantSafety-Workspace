// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           IComplianceRuleEngineDomainEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Compliance Rules Engine domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.Compliance.RulesEngine.Events;

/// <summary>Marker interface for all Compliance Rules Engine domain events.</summary>
public interface IComplianceRuleEngineDomainEvent : IDomainEvent { }
