// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           ProjectStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the Project lifecycle commands (Create, Activate,
//                  PutOnHold, Resume, Complete, Cancel, Archive, AddMilestone, AchieveMilestone).
//                  Mirrors PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Commands
//                  .RootCauseAnalysisStatusResult's precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.PlanningProjects.ProjectAggregate.Commands;

/// <summary>Result payload shared by Project lifecycle commands.</summary>
/// <param name="ProjectId">Identity of the affected Project aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record ProjectStatusResult(Guid ProjectId, string Status);
