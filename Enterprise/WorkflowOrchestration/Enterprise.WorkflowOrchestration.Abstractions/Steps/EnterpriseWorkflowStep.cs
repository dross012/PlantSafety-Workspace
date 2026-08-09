// ============================================================================
//  Project:        Enterprise.WorkflowOrchestration
//  Module:         Abstractions.Steps
//  File:           EnterpriseWorkflowStep.cs
//  Created:        2026-08-08
//  Description:    Phase D.6: a single coordinated unit of work within an
//                  orchestrated enterprise workflow. StepType is exactly
//                  the six trigger/action kinds Path D's spec requires the
//                  orchestrator to coordinate (mirrors Shared.WorkflowEngine's
//                  RequiredAction/SopTrigger/PermitTrigger/EvacuationTrigger
//                  from Phase C.7, plus two new enterprise-level kinds --
//                  Shutdown/Restart -- that operate above any single
//                  facility's own lifecycle, see Shared.FacilityDomain's
//                  Phase C.12 FacilityLifecycleStatus for the
//                  single-facility equivalent this coordinates across many).
// ============================================================================
namespace Enterprise.WorkflowOrchestration.Abstractions.Steps;

/// <summary>The kind of coordinated unit an <see cref="EnterpriseWorkflowStep"/> represents.</summary>
public enum EnterpriseWorkflowStepType
{
    /// <summary>A required action to be carried out.</summary>
    RequiredAction,

    /// <summary>A Standard Operating Procedure to be invoked.</summary>
    SopTrigger,

    /// <summary>A work permit to be secured.</summary>
    PermitTrigger,

    /// <summary>An evacuation to be carried out.</summary>
    EvacuationTrigger,

    /// <summary>An operational shutdown to be carried out.</summary>
    ShutdownTrigger,

    /// <summary>An operational restart to be carried out.</summary>
    RestartTrigger,
}

/// <summary>A single coordinated unit of work within an orchestrated enterprise workflow.</summary>
public sealed record EnterpriseWorkflowStep(
    EnterpriseWorkflowStepType StepType,
    string Code,
    string Description);
