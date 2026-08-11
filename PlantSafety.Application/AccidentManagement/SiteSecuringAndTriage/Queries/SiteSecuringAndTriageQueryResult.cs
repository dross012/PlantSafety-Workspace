// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetSiteSecuringAndTriageQuery.
// SchemaVersion: 1

using PlantSafety.Application.AccidentManagement.SiteSecuringAndTriage.Models;

namespace PlantSafety.Application.AccidentManagement.SiteSecuringAndTriage.Queries;

/// <summary>Result of <see cref="GetSiteSecuringAndTriageQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Site Securing, Injury Triage &amp; Emergency Plan Execution items.</param>
public sealed record SiteSecuringAndTriageQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SiteSecuringAndTriageItemDto> Items);