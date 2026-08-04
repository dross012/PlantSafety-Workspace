// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetCommunityContactsQuery.
// SchemaVersion: 1

using PlantSafety.Application.CommunitySafety.CommunityContact.Models;

namespace PlantSafety.Application.CommunitySafety.CommunityContact.Queries;

/// <summary>Result of <see cref="GetCommunityContactsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching CommunityContact items.</param>
public sealed record CommunityContactsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<CommunityContactItemDto> Items);
