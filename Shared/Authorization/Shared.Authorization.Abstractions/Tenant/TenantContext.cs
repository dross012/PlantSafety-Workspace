// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Tenant
//  File:           TenantContext.cs
//  Created:        2026-08-10
//  Description:    Phase 11 (Multi-Tenant & Contextual Access Modernization)
//                  deliverable. Pure-data, immutable, versioned tenant scope
//                  carried alongside a CallerContext (Phase 9/10). Formalizes
//                  the tenant/facility boundary that already exists as real,
//                  committed data -- Shared.SafetyData's ITenantFacilityRegistryV2
//                  (Phase C.8) -- rather than inventing a new tenancy model.
//
//                  Scope correction: the prompt this phase was generated from
//                  also asked for DepartmentId and MachineGroupId fields.
//                  Neither concept exists anywhere in this program's domain
//                  (Shared.FacilityDomain, Shared.EmployeeDomain,
//                  Shared.MachineDomain, Shared.SafetyData -- confirmed by
//                  direct search before writing this file). Adding them would
//                  invent business concepts this phase's own instructions
//                  ("must not introduce new business logic... formalize what
//                  already exists implicitly") explicitly forbid. Only
//                  TenantId and FacilityId are kept, matching
//                  TenantFacilityMap's real (TenantId: string, FacilityId:
//                  Guid) shape exactly -- FacilityId is a plain Guid, not
//                  Shared.FacilityDomain.FacilityId, for the same
//                  no-cross-repo-type-dependency reason TenantFacilityMap
//                  itself documents.
//
//                  Unscoped() is this phase's equivalent of CallerContext.
//                  SystemDefault(): no live caller sends a tenant-bound API
//                  key today (same standing fact as Phase 10's "no live
//                  caller sends any API key"), so every request resolves to
//                  Unscoped unless an operator configures tenant bindings.
//                  Tenant isolation rules (TenantIsolationCheck) treat
//                  Unscoped as "no tenant boundary configured yet" -- a
//                  literal absence of data, not a privileged bypass invented
//                  for convenience.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;

namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>The deterministic, pure-data tenant scope every tenant-boundary check runs against.</summary>
public sealed record TenantContext(
    string TenantId,
    Guid? FacilityId,
    CallerContext Caller,
    string CorrelationId)
{
    /// <summary>The current shape version of this record, carried in telemetry payloads for forward compatibility.</summary>
    public const int SchemaVersion = 1;

    /// <summary>The sentinel TenantId used by <see cref="Unscoped"/> -- never a real, registered tenant identifier.</summary>
    public const string UnscopedTenantId = "__unscoped__";

    /// <summary>
    /// The placeholder tenant scope used whenever no tenant binding is configured for the current
    /// caller (today, everywhere in this program -- see this file's own header comment).
    /// TenantIsolationCheck treats this as "no boundary to enforce," not a bypass.
    /// </summary>
    public static TenantContext Unscoped(CallerContext caller, string correlationId) =>
        new(UnscopedTenantId, FacilityId: null, caller, correlationId);
}
