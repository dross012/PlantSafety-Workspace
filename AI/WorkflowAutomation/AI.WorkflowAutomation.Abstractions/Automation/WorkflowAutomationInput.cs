// ============================================================================
//  Project:        AI.WorkflowAutomation
//  Module:         Abstractions.Automation
//  File:           WorkflowAutomationInput.cs
//  Created:        2026-08-08
//  Description:    Phase E.3 (Path E: AI-Driven Safety Automation): the
//                  input WorkflowAutomationEngine.Automate reasons over.
//                  Wraps Shared.WorkflowEngine's real WorkflowContext
//                  (Phase C.7) rather than re-declaring its fields, plus
//                  the two extra signals (shutdown/restart) that context
//                  doesn't carry -- those are Enterprise.WorkflowOrchestration
//                  (Phase D.6) concepts layered on top, not part of the
//                  original per-event workflow evaluation.
// ============================================================================
using Shared.WorkflowEngine.Abstractions.Engine;

namespace AI.WorkflowAutomation.Abstractions.Automation;

/// <summary>The input a workflow-automation session reasons over.</summary>
public sealed record WorkflowAutomationInput(
    WorkflowContext Context,
    bool RequiresShutdown,
    bool RequiresRestart);
