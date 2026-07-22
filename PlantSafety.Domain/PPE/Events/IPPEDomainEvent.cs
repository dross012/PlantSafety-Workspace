// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE
//  Layer:          Domain
//  File:           IPPEDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level PPE domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.Events;

/// <summary>Marker interface for top-level PPE domain events.</summary>
public interface IPPEDomainEvent : IDomainEvent { }