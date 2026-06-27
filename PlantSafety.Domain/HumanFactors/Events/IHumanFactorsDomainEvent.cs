// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Domain
//  File:           IHumanFactorsDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all HumanFactors domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.Events;

/// <summary>Marker interface for all HumanFactors domain events.</summary>
public interface IHumanFactorsDomainEvent : IDomainEvent { }
