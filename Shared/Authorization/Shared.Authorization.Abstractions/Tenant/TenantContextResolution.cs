// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Tenant
//  File:           TenantContextResolution.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Mirrors Shared.Authorization.
//                  Abstractions.Caller.CallerContextResolution (Phase 10)
//                  exactly: the resolved TenantContext plus whether an API
//                  key was provided/recognized against the tenant
//                  registration map, so callers can emit accurate telemetry
//                  and governance signals without re-deriving it.
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>The resolved TenantContext for a request, plus whether the API key was recognized against the tenant map.</summary>
public sealed record TenantContextResolution(TenantContext Context, bool TenantKeyProvided, bool TenantRecognized);
