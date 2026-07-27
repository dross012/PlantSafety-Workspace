// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-26
// FILE DESCRIPTION: Query to retrieve a single Culture record by id, scoped to a tenant.
// SchemaVersion: 1

using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Application.Analytics.Culture.Queries;

/// <summary>Retrieves a single Culture record by <paramref name="Id"/>, scoped to <paramref name="TenantId"/>.</summary>
/// <param name="Id">Identifier of the Culture record to retrieve.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
public sealed record GetCultureQuery(Guid Id, TenantId TenantId);
