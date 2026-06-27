// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Domain
//  File:           IPlanningProjectsDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all PlanningProjects domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PlanningProjects.Events;

/// <summary>Marker interface for all PlanningProjects domain events.</summary>
public interface IPlanningProjectsDomainEvent : IDomainEvent { }
