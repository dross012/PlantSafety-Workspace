// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           IAuditFindingDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all AuditFinding domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Events;

/// <summary>Marker interface for all AuditFinding domain events.</summary>
public interface IAuditFindingDomainEvent : IDomainEvent { }