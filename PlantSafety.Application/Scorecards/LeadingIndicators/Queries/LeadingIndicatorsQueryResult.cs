// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetLeadingIndicatorsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Scorecards.LeadingIndicators.Models;

namespace PlantSafety.Application.Scorecards.LeadingIndicators.Queries;

/// <summary>Result of <see cref="GetLeadingIndicatorsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Leading Indicators (Training, Audit, CAPA closure, PM compliance) items.</param>
public sealed record LeadingIndicatorsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<LeadingIndicatorsItemDto> Items);