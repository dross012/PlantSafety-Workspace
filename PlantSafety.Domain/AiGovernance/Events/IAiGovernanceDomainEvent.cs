// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Domain
//  File:           IAiGovernanceDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all AiGovernance domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiGovernance.Events;

/// <summary>Marker interface for all AiGovernance domain events.</summary>
public interface IAiGovernanceDomainEvent : IDomainEvent { }
