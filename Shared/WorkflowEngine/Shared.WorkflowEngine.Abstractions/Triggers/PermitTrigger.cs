// ============================================================================
//  Project:        Shared.WorkflowEngine
//  Module:         Abstractions.Triggers
//  File:           PermitTrigger.cs
//  Created:        2026-08-08
//  Description:    Phase C.7: a work permit requirement triggered by a
//                  workflow evaluation (e.g. a hot-work permit).
// ============================================================================
namespace Shared.WorkflowEngine.Abstractions.Triggers;

/// <summary>A work permit requirement triggered by a workflow evaluation.</summary>
public sealed record PermitTrigger(
    string PermitCode,
    string TriggerReason);
