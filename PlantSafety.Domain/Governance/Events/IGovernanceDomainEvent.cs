// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Domain
//  File:           IGovernanceDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Governance domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Governance.Events;

/// <summary>Marker interface for all Governance domain events.</summary>
public interface IGovernanceDomainEvent : IDomainEvent { }
