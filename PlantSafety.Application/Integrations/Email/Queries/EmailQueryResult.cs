// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetEmailQuery.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Email.Models;

namespace PlantSafety.Application.Integrations.Email.Queries;

/// <summary>Result of <see cref="GetEmailQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Integrations.Email items.</param>
public sealed record EmailQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EmailItemDto> Items);
