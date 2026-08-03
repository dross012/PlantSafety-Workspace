// AUTHOR: PlantSafety Platform Team (Security & Compliance Agent, Phase 7 §5.4)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query retrieving a tenant's access-log/permission-usage audit stream
//                    (Handbook Phase 6 §5.4 Compliance Monitoring). TenantId is mandatory and is
//                    the sole scoping key -- this is the "tenant-level audit stream" required by
//                    Phase 7 §5.4, composed over the EXISTING PermissionUsageLog capture
//                    (AuthorizationBehavior) rather than a new logging pipeline.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Queries;

/// <summary>
/// Retrieves the tenant-scoped authorization-decision (permission usage) audit stream for the
/// inclusive UTC range [<paramref name="FromUtc"/>, <paramref name="ToUtc"/>].
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims. Mandatory -- there is no
/// "all tenants" mode for this query, by design (Handbook Zero-Trust §5 PII &amp; Well-Being Data
/// Isolation: access logs are tenant-confidential).</param>
/// <param name="FromUtc">Inclusive UTC range start.</param>
/// <param name="ToUtc">Inclusive UTC range end.</param>
public sealed record GetTenantAuditStreamQuery(
    Guid TenantId,
    DateTime FromUtc,
    DateTime ToUtc) : IRequest<Result<TenantAuditStreamResult>>;

/// <summary>Result of <see cref="GetTenantAuditStreamQuery"/>.</summary>
/// <param name="TenantId">The tenant this stream was scoped to.</param>
/// <param name="GeneratedAtUtc">UTC timestamp when this stream snapshot was generated.</param>
/// <param name="Entries">The tenant's authorization-decision entries, newest first.</param>
public sealed record TenantAuditStreamResult(
    Guid TenantId,
    DateTime GeneratedAtUtc,
    IReadOnlyList<PermissionUsageLogEntry> Entries);
