// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Domain
//  File:           IDischargePointDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all DischargePoint domain events. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Events;

/// <summary>Marker interface for all DischargePoint domain events.</summary>
public interface IDischargePointDomainEvent : IDomainEvent { }