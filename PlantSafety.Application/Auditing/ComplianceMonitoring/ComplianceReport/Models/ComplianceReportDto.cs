// AUTHOR: PlantSafety Platform Team (Security & Compliance Agent, Phase 9 §5.6 Compliance Report Generation)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Result DTO for GetComplianceReportQuery -- composes three previously
//   independent real data sources (Handbook Phase 9 §5.6 governance-validation finding: TRIR/LTIR,
//   incident/audit lineage, and cross-site actions existed as three separate real, queryable
//   sources with no unified compliance-report query/export combining them) into a single coherent,
//   queryable/exportable compliance report. Each section is composed via the SharedKernel
//   IReportSectionProvider<,> contract (Handbook Part IX §9.3.2) from the module that actually owns
//   the data -- this DTO introduces no new data, only a read-model rollup over existing sources.
// SchemaVersion: 1

using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Models;
using PlantSafety.Application.GlobalSafetyInsights.Models;

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.ComplianceReport.Models;

/// <summary>
/// Unified compliance report combining TRIR/LTIR KPIs, the tenant's audit-trail/incident-lineage
/// stream, and cross-site coordinated-action history for a single tenant and date range.
/// </summary>
/// <param name="TenantId">The tenant this report was generated for.</param>
/// <param name="FromUtc">Inclusive UTC start of the report's date range.</param>
/// <param name="ToUtc">Inclusive UTC end of the report's date range.</param>
/// <param name="GeneratedAtUtc">UTC instant this report snapshot was generated.</param>
/// <param name="SafetyKpi">
/// Most recently computed TRIR/LTIR KPI snapshot within the range (from
/// <c>GlobalSafetyKpiEngine</c>/<c>GlobalSafetyInsightsAggregate</c>), or <see langword="null"/> when
/// no snapshot has ever been computed for this tenant -- a real absence, not a retrieval failure.
/// </param>
/// <param name="SafetyKpiSectionAvailable">
/// <see langword="false"/> when the TRIR/LTIR section could not be retrieved (a real failure, e.g.
/// the read-model store was unavailable) -- distinct from <paramref name="SafetyKpi"/> being
/// <see langword="null"/> because no snapshot has ever been computed.
/// </param>
/// <param name="AuditTrail">
/// The tenant's audit-trail/incident-lineage entries within the range (from the
/// PermissionUsageLog-backed audit stream, which captures every authorization-gated
/// state-changing operation -- including incident lifecycle transitions -- via
/// <c>AuthorizationBehavior</c>), newest first.
/// </param>
/// <param name="AuditTrailSectionAvailable">
/// <see langword="false"/> when the audit-trail section could not be retrieved.
/// </param>
/// <param name="CrossSiteActions">Cross-site coordinated-action audit ledger entries within the range, newest first.</param>
/// <param name="CrossSiteActionsSectionAvailable">
/// <see langword="false"/> when the cross-site-actions section could not be retrieved.
/// </param>
public sealed record ComplianceReportDto(
    Guid TenantId,
    DateTime FromUtc,
    DateTime ToUtc,
    DateTime GeneratedAtUtc,
    GlobalSafetyInsightsItemDto? SafetyKpi,
    bool SafetyKpiSectionAvailable,
    IReadOnlyList<PermissionUsageLogEntry> AuditTrail,
    bool AuditTrailSectionAvailable,
    IReadOnlyList<CrossSiteActionLogEntryDto> CrossSiteActions,
    bool CrossSiteActionsSectionAvailable);
