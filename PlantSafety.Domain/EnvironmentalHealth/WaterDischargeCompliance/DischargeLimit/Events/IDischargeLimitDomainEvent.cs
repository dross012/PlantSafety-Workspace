// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           IDischargeLimitDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all DischargeLimit domain events. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Events;

/// <summary>Marker interface for all DischargeLimit domain events.</summary>
public interface IDischargeLimitDomainEvent : IDomainEvent { }