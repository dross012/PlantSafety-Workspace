// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetIncidentInvestigationCorrectiveActionQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.IncidentInvestigationCorrectiveAction.Models;

namespace PlantSafety.Application.SafetyPolicies.IncidentInvestigationCorrectiveAction.Queries;

/// <summary>Result of <see cref="GetIncidentInvestigationCorrectiveActionQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Incident Investigation &amp; Corrective Action Policies items.</param>
public sealed record IncidentInvestigationCorrectiveActionQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<IncidentInvestigationCorrectiveActionItemDto> Items);