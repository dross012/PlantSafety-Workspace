// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Domain
//  File:           IPermitSchedulingDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all PermitScheduling domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitScheduling.Events;

/// <summary>Marker interface for all PermitScheduling domain events.</summary>
public interface IPermitSchedulingDomainEvent : IDomainEvent { }