// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           IAuditEngagementDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all AuditEngagement domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Events;

/// <summary>Marker interface for all AuditEngagement domain events.</summary>
public interface IAuditEngagementDomainEvent : IDomainEvent { }