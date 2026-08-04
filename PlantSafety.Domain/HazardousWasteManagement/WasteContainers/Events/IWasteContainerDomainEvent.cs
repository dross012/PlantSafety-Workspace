// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           IWasteContainerDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Marker interface for all WasteContainer domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Events;

/// <summary>Marker interface for all WasteContainer domain events.</summary>
public interface IWasteContainerDomainEvent : IDomainEvent { }
