// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Domain
//  File:           IAiSafetyIntelligenceDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all AiSafetyIntelligence domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiSafetyIntelligence.Events;

/// <summary>Marker interface for all AiSafetyIntelligence domain events.</summary>
public interface IAiSafetyIntelligenceDomainEvent : IDomainEvent { }
