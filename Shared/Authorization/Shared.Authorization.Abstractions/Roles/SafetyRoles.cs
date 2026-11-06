// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Roles
//  Layer:          Abstractions
//  File:           SafetyRoles.cs
//  Created:        2026-07-27
//  Author:         Security & Compliance Agent
//  Description:    Canonical role-claim constants for the PlantSafety/Reporting
//                  Safety domain. Introduced to stop role-name string drift across
//                  modules (see docs/architecture/authorization-tiers.md).
// ============================================================================
//
// Provenance / evidence this tier model is grounded in:
// - PlantSafety.Api/Extensions/AuthorizationPolicyExtensions.cs registered ~34
//   domain-level policies of the form RequireRole("<Domain>.<Verb>", "Platform.Admin")
//   for Analytics/Compliance/ContinuousImprovement/Integrations/Intelligence/
//   Operational/SafetyEvents/Webhooks — establishing "Platform.Admin" as the
//   existing universal override role and a Read/Create/Update/Approve/Delete/Export
//   verb vocabulary.
// - ~140 per-bounded-context "<Module>AuthorizationPolicies" classes (e.g.
//   IncidentManagementAuthorizationPolicies) each expose only a Read/Write pair —
//   i.e. two tiers is the real granularity used almost everywhere in the modern
//   (V1/V2, Authorization/ folder) controllers.
// - PlantSafety.Api/Extensions/ServiceCollectionExtensions.cs' "RequireDashboardRead"
//   policy additionally distinguishes an approval-adjacent set of named operational
//   roles (PlantManager, SafetyOfficer, Supervisor, ComplianceAuditor, ...) from a
//   bare "PlatformAdministrator" role — the same admin concept as "Platform.Admin"
//   above, just spelled differently. That spelling drift is fixed by this file:
//   SafetyRoles.PlatformAdmin is now the single canonical literal ("Platform.Admin");
//   RequireDashboardRead has been updated to reference it instead of the
//   "PlatformAdministrator" string it previously used ad hoc.
// - ~57 legacy controllers (AlertsV1Controller, RiskManagementV1Controller,
//   AccidentManagementV1Controller, etc. — see docs/architecture/authorization-tiers.md
//   for the full list) had ONLY "RequireAuthenticatedUser" at the controller level,
//   with no distinction between read (GET) and state-changing (POST
//   create/approve/close/escalate/dispatch/resolve) operations. The 3rd tier below
//   (Approver) is introduced specifically because those controllers' own action verbs
//   (Approve/Close/Escalate/Dispatch/Resolve/Reject/Cancel) already imply a
//   higher-privilege operation than a plain create/update, mirroring the "Approve"
//   verb already present in the domain-level policy set.
//
// Result: a 4-tier model — Viewer / Editor / Approver / PlatformAdmin — is the
// smallest set that reconciles both the 2-tier (Read/Write) modern convention and
// the Approve-as-distinct-verb convention already present in the codebase, without
// inventing tiers nothing in the code implies.
//
// ============================================================================

namespace Shared.Authorization.Abstractions.Roles;

/// <summary>
/// Canonical role-claim string constants for the Safety domain (PlantSafety and its
/// Reporting sibling). All [Authorize(Roles = ...)] / RequireRole(...) usages across
/// PlantSafety.Api and Reporting.Api should reference these constants instead of
/// inline string literals, to prevent role-name drift across modules.
/// </summary>
public static class SafetyRoles
{
    /// <summary>Read-only access to Safety-domain data. Lowest tier.</summary>
    public const string Viewer = "Safety.Viewer";

    /// <summary>Create/update access to Safety-domain records (standard write operations).</summary>
    public const string Editor = "Safety.Editor";

    /// <summary>
    /// Elevated write access for state-transition operations that carry safety or
    /// regulatory weight: approve, close, escalate, dispatch, resolve, reject, cancel.
    /// </summary>
    public const string Approver = "Safety.Approver";

    /// <summary>
    /// Full administrative override. Canonical spelling of the role previously
    /// inconsistently written "Platform.Admin" (AuthorizationPolicyExtensions.cs)
    /// vs "PlatformAdministrator" (ServiceCollectionExtensions.cs). This constant's
    /// value ("Platform.Admin") was chosen because it is the spelling already used by
    /// the majority (~140) of per-module policy registrations; the minority usage
    /// ("PlatformAdministrator") has been updated to match.
    /// </summary>
    public const string PlatformAdmin = "Platform.Admin";

    /// <summary>Viewer and every tier above it. Use for read (GET) endpoints.</summary>
    public static readonly IReadOnlyList<string> ViewerAndAbove =
        new[] { Viewer, Editor, Approver, PlatformAdmin };

    /// <summary>Editor and every tier above it. Use for standard create/update (POST/PUT) endpoints.</summary>
    public static readonly IReadOnlyList<string> EditorAndAbove =
        new[] { Editor, Approver, PlatformAdmin };

    /// <summary>Approver and every tier above it. Use for approve/close/escalate/dispatch/resolve endpoints.</summary>
    public static readonly IReadOnlyList<string> ApproverAndAbove =
        new[] { Approver, PlatformAdmin };

    /// <summary>PlatformAdmin only. Use for destructive (delete), platform-management, and backup/restore endpoints.</summary>
    public static readonly IReadOnlyList<string> AdminOnly =
        new[] { PlatformAdmin };
}
