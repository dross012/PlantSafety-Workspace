// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           IWorkflowInstanceScopedCommand.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code)
//  SchemaVersion:  2026.1
//  Description:    Marker interface (mirroring IConcurrencyAwareCommand's identical precedent in
//                  PlantSafety.Application.Behaviors.Concurrency.ConcurrencyBehavior) for Workflow
//                  commands whose handler performs a read-modify-write cycle (load
//                  WorkflowInstance, apply a state transition, persist) against a single
//                  WorkflowInstance aggregate. WorkflowInstanceLockBehavior wraps every command
//                  implementing this interface in a SQL Server distributed lock keyed by
//                  WorkflowInstanceId, serializing concurrent/racing mutation of the same
//                  instance across handler invocations, requests, and process instances.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Workflow.Commands;

/// <summary>
/// Marker interface for Workflow commands that mutate a single <c>WorkflowInstance</c> aggregate
/// via a load-mutate-persist cycle. See <c>WorkflowInstanceLockBehavior</c>.
/// </summary>
public interface IWorkflowInstanceScopedCommand
{
    /// <summary>Identity of the WorkflowInstance aggregate this command mutates.</summary>
    Guid WorkflowInstanceId { get; }
}
