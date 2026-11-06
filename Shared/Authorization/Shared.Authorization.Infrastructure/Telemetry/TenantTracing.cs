// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Telemetry
//  File:           TenantTracing.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Named, deterministic span-name
//                  constants for the two tenant operations this phase
//                  introduces, mirroring AuthenticationTracing (Phase 10)
//                  exactly -- not a new tracing primitive.
// ============================================================================
namespace Shared.Authorization.Infrastructure.Telemetry;

/// <summary>Canonical, deterministic span/operation names for the tenant layer.</summary>
public static class TenantTracing
{
    /// <summary>The operation name for resolving an incoming request's API key into a TenantContext.</summary>
    public const string TenantResolutionSpanName = "Tenant.ResolveTenant";

    /// <summary>The operation name for evaluating a tenant-isolation check against a requested facility.</summary>
    public const string TenantIsolationCheckSpanName = "Tenant.EvaluateIsolation";
}
