// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety
//  Layer:          Domain
//  File:           IEmployeeSafetyDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level EmployeeSafety domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Events;

/// <summary>Marker interface for top-level EmployeeSafety domain events.</summary>
public interface IEmployeeSafetyDomainEvent : IDomainEvent { }