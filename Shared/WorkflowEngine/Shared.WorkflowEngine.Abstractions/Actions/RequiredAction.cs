// ============================================================================
//  Project:        Shared.WorkflowEngine
//  Module:         Abstractions.Actions
//  File:           RequiredAction.cs
//  Created:        2026-08-08
//  Description:    Phase C.7 (Path C: Operational Modernization): a single
//                  required action produced by WorkflowEngine.Evaluate.
// ============================================================================
namespace Shared.WorkflowEngine.Abstractions.Actions;

/// <summary>A required action produced by a workflow evaluation.</summary>
public sealed record RequiredAction(
    string ActionCode,
    string Description,
    int DueWithinHours);
