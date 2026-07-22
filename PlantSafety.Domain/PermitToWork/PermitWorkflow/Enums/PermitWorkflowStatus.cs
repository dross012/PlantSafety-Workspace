// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowStatus.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the PermitWorkflow aggregate root, modeling the full
//                  Permit-to-Work (PTW) process described in the PlantSafety Program Structure
//                  manual, Section "Permit Workflow (Full Process)" (Steps 1-7):
//                  Request -> Hazard Assessment -> Control Implementation -> Authorization ->
//                  Work Execution -> Close-Out -> Review & Archive.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Enums;

/// <summary>
/// Lifecycle states for the <see cref="PermitWorkflow"/> aggregate root, modeling the full
/// Permit-to-Work process (request, hazard assessment, control implementation, authorization,
/// execution, close-out, review/archive).
/// </summary>
public enum PermitWorkflowStatus
{
    /// <summary>Step 1: worker or contractor has submitted a permit request; training/authorization not yet verified.</summary>
    Requested = 0,

    /// <summary>Step 2: safety officer has completed the hazard assessment and identified required controls.</summary>
    HazardAssessed = 1,

    /// <summary>Step 3: required controls (LOTO, PPE, barricades, atmospheric testing, etc.) have been implemented.</summary>
    ControlsImplemented = 2,

    /// <summary>Step 4: supervisor and safety officer have authorized the permit.</summary>
    Approved = 3,

    /// <summary>Step 4/5: permit has been issued by the issuer and work is active in the field.</summary>
    Active = 4,

    /// <summary>Step 6: work area has been inspected/closed out and awaits final closure signature.</summary>
    PendingClosureSignature = 5,

    /// <summary>Step 6: permit has been closed with a recorded closure signature.</summary>
    Closed = 6,

    /// <summary>Step 7: permit has been stored for compliance review and long-term retention.</summary>
    Archived = 7,

    /// <summary>Permit request was declined during hazard assessment, control implementation, or authorization.</summary>
    Rejected = 8,

    /// <summary>Active permit passed its validity period without being closed and was automatically expired.</summary>
    Expired = 9,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Requested"/>.</summary>
    [Obsolete("Use Requested. Retained only for additive-only compatibility with prior scaffolding.")]
    Created = Requested,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Closed"/>.</summary>
    [Obsolete("Use Closed. Retained only for additive-only compatibility with prior scaffolding.")]
    Completed = Closed,
}
