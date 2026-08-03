// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           IAuditTimelineDomainEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all AuditTimeline submodule domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.Auditing.AuditTimeline.Events;

/// <summary>Marker interface for all AuditTimeline submodule domain events.</summary>
public interface IAuditTimelineDomainEvent : IDomainEvent { }
