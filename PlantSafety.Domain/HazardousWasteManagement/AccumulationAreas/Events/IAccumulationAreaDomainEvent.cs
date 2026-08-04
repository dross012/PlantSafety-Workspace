// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           IAccumulationAreaDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Marker interface for all AccumulationArea domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Events;

/// <summary>Marker interface for all AccumulationArea domain events.</summary>
public interface IAccumulationAreaDomainEvent : IDomainEvent { }
