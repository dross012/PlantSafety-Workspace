// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.PlanningProjects
//  Layer:          SharedKernel
//  File:           IPlanningProjectsIntegrationEvent.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all PlanningProjects integration events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Events;

namespace PlantSafety.SharedKernel.PlanningProjects.Events;

/// <summary>
/// Marker interface for PlanningProjects-area integration events.
/// </summary>
public interface IPlanningProjectsIntegrationEvent : IIntegrationEvent { }
