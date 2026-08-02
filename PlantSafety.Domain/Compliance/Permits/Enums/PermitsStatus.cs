// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           PermitsStatus.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Full Permit-to-Work lifecycle statuses for the Permits aggregate.
//  UPDATED:        2026-07-30 -- Phase 4 full build-out: added Rejected/Expired (additive; 0-4
//                  unchanged) so the aggregate can model a genuine PTW lifecycle (a permit request
//                  can be declined before approval, and an approved-but-unclosed permit can expire)
//                  instead of only the generic Created/Active/Completed/Closed/Archived shape it
//                  had before any lifecycle methods existed. Mirrors PermitToWork.PermitWorkflow's
//                  Reject/Expire paths.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Enums;

/// <summary>Lifecycle states for the <see cref="Permits"/> aggregate root.</summary>
public enum PermitsStatus
{
    /// <summary>Permit has been requested and awaits approval.</summary>
    Created = 0,

    /// <summary>Permit has been approved and issued; work may proceed.</summary>
    Active = 1,

    /// <summary>Permitted work has been completed.</summary>
    Completed = 2,

    /// <summary>Permit has been closed with no further action required.</summary>
    Closed = 3,

    /// <summary>Permit has been archived for long-term compliance retention.</summary>
    Archived = 4,

    /// <summary>Permit request was declined before approval.</summary>
    Rejected = 5,

    /// <summary>An issued permit passed its expiry window without being completed/closed.</summary>
    Expired = 6,
}
