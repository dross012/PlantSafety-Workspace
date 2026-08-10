// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Roles
//  File:           EnterpriseRole.cs
//  Created:        2026-08-10
//  Description:    Phase 9 (Authorization Modernization) deliverable, built
//                  now (Phase 14) to satisfy the five phases (9-13) already
//                  merged to main that reference Shared.Authorization
//                  without it ever having been created -- see this
//                  project's own README for the full record. A plain,
//                  additive-only enum, matching the established convention
//                  every other shared vocabulary in this program uses
//                  (Shared.FacilityDomain.Abstractions.Severity.SeverityLevel,
//                  Shared.FacilityDomain.Abstractions.Hazards.HazardType) --
//                  not a "smart enum" class, despite EnterpriseRole being
//                  referenced only via simple member access (EnterpriseRole.
//                  PlatformAdmin, EnterpriseRole.Viewer) everywhere it is
//                  actually used in code. Ordered from least to most
//                  privileged; EnterprisePermissionResolver (Permissions/)
//                  maps each tier to the ModulePermissionVerb set it grants.
// ============================================================================
namespace Shared.Authorization.Abstractions.Roles;

/// <summary>The enterprise-wide role vocabulary, ordered from least to most privileged.</summary>
public enum EnterpriseRole
{
    /// <summary>Read-only access to every module.</summary>
    Viewer,

    /// <summary>Read and write access to every module's day-to-day operations.</summary>
    Operator,

    /// <summary>Read, write, and administrative access scoped to facility-level operations.</summary>
    FacilityManager,

    /// <summary>Full read, write, and administrative access across every module and tenant.</summary>
    PlatformAdmin,
}
