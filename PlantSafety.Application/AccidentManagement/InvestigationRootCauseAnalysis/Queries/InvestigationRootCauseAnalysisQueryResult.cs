// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetInvestigationRootCauseAnalysisQuery.
// SchemaVersion: 1

using PlantSafety.Application.AccidentManagement.InvestigationRootCauseAnalysis.Models;

namespace PlantSafety.Application.AccidentManagement.InvestigationRootCauseAnalysis.Queries;

/// <summary>Result of <see cref="GetInvestigationRootCauseAnalysisQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Investigation &amp; Root Cause Analysis items.</param>
public sealed record InvestigationRootCauseAnalysisQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<InvestigationRootCauseAnalysisItemDto> Items);