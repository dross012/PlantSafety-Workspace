// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Contracts
//  File:           ContractMetadata.cs
//  Created:        2026-08-10
//  Description:    Phase 12 (Unified Contract & Schema Modernization)
//                  deliverable. A pure-data bundle of the five metadata
//                  fields every schema in this program already carries in
//                  some form -- tenant (Phase 11), caller role + MFA status
//                  (Phase 9), correlation id (Phase 2.4), timestamp (Phase
//                  2.1's IClock) -- composed into one immutable, versioned
//                  record for callers that want them together. Field order
//                  is alphabetical (CallerRole, CorrelationId, MfaStatus,
//                  TenantId, TimestampUtc), per this phase's deterministic-
//                  field-ordering rule; this is a brand-new type with no
//                  prior positional convention to break.
//
//                  Deliberately excludes "CallerPermissions": no such field
//                  exists on CallerContext (Phase 9) or anywhere in
//                  Shared.Authorization. Permissions are resolved on demand
//                  per module via IPermissionResolver.Resolve(role, module) --
//                  adding a static Permissions field here would invent a
//                  type nothing in this program has, the same correction
//                  already made in Phases 9 and 10.
// ============================================================================
namespace Shared.Authorization.Abstractions.Contracts;

/// <summary>The unified metadata bundle (tenant, caller role, MFA status, correlation, timestamp) every contract carries.</summary>
public sealed record ContractMetadata(
    string CallerRole,
    string CorrelationId,
    Caller.MfaStatus MfaStatus,
    string TenantId,
    DateTime TimestampUtc)
{
    /// <summary>The current shape version of this record.</summary>
    public const int SchemaVersion = 1;
}
