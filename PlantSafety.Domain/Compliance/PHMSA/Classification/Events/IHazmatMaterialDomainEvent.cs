// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           IHazmatMaterialDomainEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all HazmatMaterial domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Events;

/// <summary>Marker interface for all HazmatMaterial domain events.</summary>
public interface IHazmatMaterialDomainEvent : IDomainEvent { }
