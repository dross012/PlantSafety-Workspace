// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Domain
//  File:           ICrisisManagementDomainEvent.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all CrisisManagement domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.CrisisManagement.Events;

/// <summary>Marker interface for all CrisisManagement domain events.</summary>
public interface ICrisisManagementDomainEvent : IDomainEvent { }
