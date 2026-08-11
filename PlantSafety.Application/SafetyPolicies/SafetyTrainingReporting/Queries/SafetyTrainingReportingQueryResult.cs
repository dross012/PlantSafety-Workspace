// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetSafetyTrainingReportingQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.SafetyTrainingReporting.Models;

namespace PlantSafety.Application.SafetyPolicies.SafetyTrainingReporting.Queries;

/// <summary>Result of <see cref="GetSafetyTrainingReportingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Safety Training &amp; Reporting Policies items.</param>
public sealed record SafetyTrainingReportingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SafetyTrainingReportingItemDto> Items);