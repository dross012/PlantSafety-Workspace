// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           IPPEZoneMappingDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all PPEZoneMapping domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.Events;

/// <summary>Marker interface for all PPEZoneMapping domain events.</summary>
public interface IPPEZoneMappingDomainEvent : IDomainEvent { }