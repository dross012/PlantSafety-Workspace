// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           IAuditReportDomainEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all AuditReport submodule domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.Auditing.AuditReport.Events;

/// <summary>Marker interface for all AuditReport submodule domain events.</summary>
public interface IAuditReportDomainEvent : IDomainEvent { }
