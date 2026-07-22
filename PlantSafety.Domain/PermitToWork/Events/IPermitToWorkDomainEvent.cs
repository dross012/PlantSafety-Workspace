// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork
//  Layer:          Domain
//  File:           IPermitToWorkDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level PermitToWork domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.Events;

/// <summary>Marker interface for top-level PermitToWork domain events.</summary>
public interface IPermitToWorkDomainEvent : IDomainEvent { }