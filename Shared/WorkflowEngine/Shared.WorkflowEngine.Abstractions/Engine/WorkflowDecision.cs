// ============================================================================
//  Project:        Shared.WorkflowEngine
//  Module:         Abstractions.Engine
//  File:           WorkflowDecision.cs
//  Created:        2026-08-08
//  Description:    Phase C.7: the deterministic output of WorkflowEngine.Evaluate
//                  -- every required action, SOP/permit/evacuation trigger,
//                  and notification routing decided for a given WorkflowContext.
// ============================================================================
using Shared.WorkflowEngine.Abstractions.Actions;
using Shared.WorkflowEngine.Abstractions.Notification;
using Shared.WorkflowEngine.Abstractions.Triggers;

namespace Shared.WorkflowEngine.Abstractions.Engine;

/// <summary>The deterministic output of a workflow evaluation.</summary>
public sealed record WorkflowDecision(
    IReadOnlyList<RequiredAction> RequiredActions,
    IReadOnlyList<SopTrigger> SopTriggers,
    IReadOnlyList<PermitTrigger> PermitTriggers,
    IReadOnlyList<EvacuationTrigger> EvacuationTriggers,
    IReadOnlyList<NotificationRouting> NotificationRoutings);
