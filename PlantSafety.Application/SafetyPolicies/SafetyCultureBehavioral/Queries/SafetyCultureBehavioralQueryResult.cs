// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetSafetyCultureBehavioralQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.SafetyCultureBehavioral.Models;

namespace PlantSafety.Application.SafetyPolicies.SafetyCultureBehavioral.Queries;

/// <summary>Result of <see cref="GetSafetyCultureBehavioralQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Safety Culture &amp; Behavioral Policies items.</param>
public sealed record SafetyCultureBehavioralQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SafetyCultureBehavioralItemDto> Items);