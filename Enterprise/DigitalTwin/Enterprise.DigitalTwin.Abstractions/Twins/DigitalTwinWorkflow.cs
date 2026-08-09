// ============================================================================
//  Project:        Enterprise.DigitalTwin
//  Module:         Abstractions.Twins
//  File:           DigitalTwinWorkflow.cs
//  Created:        2026-08-08
//  Description:    Phase D.7: a read-only virtual snapshot of a workflow's
//                  current step, for simulation purposes only.
// ============================================================================
namespace Enterprise.DigitalTwin.Abstractions.Twins;

/// <summary>A read-only virtual snapshot of a workflow's current step.</summary>
public sealed record DigitalTwinWorkflow(
    Guid WorkflowId,
    string CurrentStepDescription);
