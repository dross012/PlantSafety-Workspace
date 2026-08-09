// ============================================================================
//  Project:        Shared.WorkflowEngine
//  Module:         Abstractions.Triggers
//  File:           SopTrigger.cs
//  Created:        2026-08-08
//  Description:    Phase C.7: a Standard Operating Procedure triggered by a
//                  workflow evaluation.
// ============================================================================
namespace Shared.WorkflowEngine.Abstractions.Triggers;

/// <summary>A Standard Operating Procedure triggered by a workflow evaluation.</summary>
public sealed record SopTrigger(
    string SopCode,
    string TriggerReason);
