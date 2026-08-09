// ============================================================================
//  Project:        Shared.WorkflowEngine
//  Module:         Abstractions.Engine
//  File:           WorkflowEngine.cs
//  Created:        2026-08-08
//  Description:    Phase C.7 (Path C: Operational Modernization): the
//                  real-time operational workflow engine. Deterministically
//                  reacts to SeverityLevel, HazardType, facility-hierarchy
//                  scope, employee exposure, machine failure, and
//                  multi-hazard chain length -- exactly the six inputs
//                  Path C's spec calls out -- producing required actions,
//                  SOP/permit/evacuation triggers, and notification
//                  routings. Pure function, no side effects, no reflection.
// ============================================================================
using Shared.EmployeeDomain.Abstractions.Enums;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

using Shared.WorkflowEngine.Abstractions.Actions;
using Shared.WorkflowEngine.Abstractions.Notification;
using Shared.WorkflowEngine.Abstractions.Triggers;

namespace Shared.WorkflowEngine.Abstractions.Engine;

/// <summary>Deterministically evaluates a <see cref="WorkflowContext"/> into a <see cref="WorkflowDecision"/>.</summary>
public static class WorkflowEngine
{
    /// <summary>Evaluates the given context and returns every required action/trigger/notification it deterministically implies.</summary>
    public static WorkflowDecision Evaluate(WorkflowContext context)
    {
        var requiredActions = new List<RequiredAction>();
        var sopTriggers = new List<SopTrigger>();
        var permitTriggers = new List<PermitTrigger>();
        var evacuationTriggers = new List<EvacuationTrigger>();
        var notificationRoutings = new List<NotificationRouting>();

        if (context.SeverityLevel >= SeverityLevel.Severe)
        {
            requiredActions.Add(new RequiredAction("EMERGENCY-RESPONSE", "Initiate emergency response.", DueWithinHours: 0));
            evacuationTriggers.Add(new EvacuationTrigger(
                context.SeverityLevel == SeverityLevel.Catastrophic ? EvacuationScope.Facility : context.AffectedScope,
                $"{context.SeverityLevel} severity {context.HazardType} event."));
        }

        switch (context.HazardType)
        {
            case HazardType.Fire:
                sopTriggers.Add(new SopTrigger("FIRE-RESPONSE-SOP", "Fire hazard detected."));
                break;
            case HazardType.ChemicalSpill:
            case HazardType.EnvironmentalRelease:
                sopTriggers.Add(new SopTrigger("SPILL-CONTAINMENT-SOP", "Chemical/environmental release detected."));
                break;
            case HazardType.HotWork:
                permitTriggers.Add(new PermitTrigger("HOT-WORK-PERMIT", "Hot work activity requires an active permit."));
                break;
            case HazardType.ConfinedSpace:
                permitTriggers.Add(new PermitTrigger("CONFINED-SPACE-PERMIT", "Confined space entry requires an active permit."));
                break;
            case HazardType.LOTOFailure:
                sopTriggers.Add(new SopTrigger("LOTO-SOP", "Lockout/tagout failure detected."));
                break;
        }

        if (context.MachineFailureCount > 0)
        {
            requiredActions.Add(new RequiredAction("EQUIPMENT-ISOLATION", "Isolate and inspect the failed equipment.", DueWithinHours: 2));
            sopTriggers.Add(new SopTrigger("LOTO-SOP", "Machine failure requires energy isolation before inspection."));
        }

        if (context.EmployeeExposureCount > 0)
        {
            requiredActions.Add(new RequiredAction("MEDICAL-EVALUATION", "Arrange medical evaluation for exposed personnel.", DueWithinHours: 4));
        }

        if (context.EventChainLength >= 3)
        {
            requiredActions.Add(new RequiredAction("MULTI-HAZARD-ESCALATION", "Escalate: event has propagated across 3 or more hazard stages.", DueWithinHours: 1));
        }

        notificationRoutings.Add(new NotificationRouting(EmployeeRole.Safety, NotificationChannel.Email));

        if (context.SeverityLevel >= SeverityLevel.Severe)
        {
            notificationRoutings.Add(new NotificationRouting(EmployeeRole.Supervisor, NotificationChannel.Sms));
        }

        if (context.SeverityLevel == SeverityLevel.Catastrophic)
        {
            notificationRoutings.Add(new NotificationRouting(EmployeeRole.Administrator, NotificationChannel.Page));
        }

        return new WorkflowDecision(requiredActions, sopTriggers, permitTriggers, evacuationTriggers, notificationRoutings);
    }
}
