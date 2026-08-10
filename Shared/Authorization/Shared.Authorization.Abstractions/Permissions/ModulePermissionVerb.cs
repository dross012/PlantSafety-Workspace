// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Permissions
//  File:           ModulePermissionVerb.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. The permission-tier vocabulary every
//                  module's *Authorization.cs checks against. Every
//                  read-only domain query in this program (WorkflowEngine.
//                  Evaluate, EventRoutingEngine.Route, SchedulingScheduleCalculator.
//                  Calculate, ...) requires Read, the lowest tier -- not an
//                  invented permission, the correct existing verb for a
//                  read-only operation, per every *Authorization.cs file's
//                  own documented reasoning.
// ============================================================================
namespace Shared.Authorization.Abstractions.Permissions;

/// <summary>A module-scoped permission tier, ordered from least to most privileged.</summary>
public enum ModulePermissionVerb
{
    /// <summary>Read-only access to a module's data or deterministic evaluations.</summary>
    Read,

    /// <summary>Access to a module's state-changing operations.</summary>
    Write,

    /// <summary>Access to a module's administrative operations. Requires MfaStatus.Verified.</summary>
    Admin,
}
