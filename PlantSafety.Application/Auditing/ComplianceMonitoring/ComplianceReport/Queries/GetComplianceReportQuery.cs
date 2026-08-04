// AUTHOR: PlantSafety Platform Team (Security & Compliance Agent, Phase 9 §5.6 Compliance Report Generation)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query composing a unified compliance report for a tenant/date range out of
//   three previously independent real data sources: TRIR/LTIR KPIs (GlobalSafetyKpiEngine /
//   GlobalSafetyInsightsAggregate), the tenant's audit-trail/incident-lineage stream
//   (PermissionUsageLog), and cross-site coordinated-action history (CrossSiteActionLog). A pure
//   composition query over existing modules' own Application-layer read models -- no new bounded
//   context, no new persistence, additive-only.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Auditing.ComplianceMonitoring.ComplianceReport.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.ComplianceReport.Queries;

/// <summary>
/// Retrieves a unified compliance report -- TRIR/LTIR KPIs, audit-trail/incident lineage, and
/// cross-site coordinated-action history -- for <paramref name="TenantId"/> within the inclusive
/// UTC range [<paramref name="FromUtc"/>, <paramref name="ToUtc"/>].
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims. Mandatory -- no "all tenants" mode.</param>
/// <param name="FromUtc">Inclusive UTC range start.</param>
/// <param name="ToUtc">Inclusive UTC range end.</param>
public sealed record GetComplianceReportQuery(
    Guid TenantId,
    DateTime FromUtc,
    DateTime ToUtc) : IRequest<Result<ComplianceReportDto>>;
