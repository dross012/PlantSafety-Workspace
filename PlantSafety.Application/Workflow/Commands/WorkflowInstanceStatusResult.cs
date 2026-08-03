// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           WorkflowInstanceStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the WorkflowInstance lifecycle-transition commands
//                  (Advance, Approve, Reject, Escalate, Retry, Cancel, MakeAutomatedDecision,
//                  SurfaceRecommendation). Mirrors
//                  PlantSafety.Application.Operational.PreventiveMaintenance.Commands.PreventiveMaintenanceTaskStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Result payload shared by WorkflowInstance lifecycle-transition commands.</summary>
/// <param name="WorkflowInstanceId">Identity of the affected WorkflowInstance aggregate.</param>
/// <param name="State">Overall lifecycle state after the transition.</param>
/// <param name="CurrentStepId">Identity of the currently active step after the transition, or null if the instance reached a terminal state.</param>
public sealed record WorkflowInstanceStatusResult(Guid WorkflowInstanceId, string State, Guid? CurrentStepId);
