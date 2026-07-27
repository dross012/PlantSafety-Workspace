// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           TenantContextPropagationOptions.cs
//  Created:        2026-07-26
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Rollout switch for the tenant-isolation remediation (Lifecycle Coverage
//                  Audit, docs\lifecycle\security\rls-audit.md). While OFF, DapperQueryExecutor
//                  /DapperCommandExecutor behave exactly as they do today (SESSION_CONTEXT is
//                  best-effort, no-ops silently when a call's parameters carry no TenantId).
//                  While ON, every call through those two chokepoints MUST carry a TenantId
//                  parameter or it throws TenantContextRequiredException -- this turns "flip
//                  the flag on in staging/canary" into a hard verification gate that every
//                  repository in scope has actually been migrated, rather than a soft, easy-to-
//                  miss behavioral change. Bound from configuration section
//                  "Persistence:TenantContextPropagation"; defaults to OFF so introducing this
//                  type has zero effect on running environments until explicitly enabled.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Dapper;

/// <summary>
/// Rollout switch for enforcing <c>TenantId</c> propagation through
/// <see cref="Queries.DapperQueryExecutor"/>/<see cref="Commands.DapperCommandExecutor"/>.
/// See remarks on <see cref="EnableTenantContextPropagation"/> for the OFF/ON contract.
/// </summary>
public sealed class TenantContextPropagationOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "Persistence:TenantContextPropagation";

    /// <summary>
    /// <para><b>OFF (default):</b> current production behavior, unchanged. SESSION_CONTEXT is
    /// set best-effort from whatever <c>TenantId</c> the call's parameters happen to carry;
    /// calls that carry none simply leave SESSION_CONTEXT unset, exactly as today.</para>
    /// <para><b>ON:</b> every call through <see cref="Queries.DapperQueryExecutor"/> or
    /// <see cref="Commands.DapperCommandExecutor"/> must carry a non-empty <c>TenantId</c>
    /// parameter, or the call throws <see cref="Exceptions.TenantContextRequiredException"/>
    /// before touching the database. Intended to be enabled first in integration tests, then a
    /// staging environment, then a single canary tenant, then globally -- per the staged
    /// repository-shape rollout plan -- never flipped directly to ON in an unverified production
    /// environment.</para>
    /// </summary>
    public bool EnableTenantContextPropagation { get; init; }
}
