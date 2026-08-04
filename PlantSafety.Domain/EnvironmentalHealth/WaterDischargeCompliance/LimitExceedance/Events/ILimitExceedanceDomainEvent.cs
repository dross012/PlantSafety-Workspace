// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           ILimitExceedanceDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all LimitExceedance domain events. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Events;

/// <summary>Marker interface for all LimitExceedance domain events.</summary>
public interface ILimitExceedanceDomainEvent : IDomainEvent { }