// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query to list every registered tenant. Platform-admin-only, cross-tenant
//   operation (there is no "TenantId" to scope this query by -- it lists tenants themselves).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Models;

namespace PlantSafety.Application.Tenancy.Tenant.Queries;

/// <summary>Lists every registered tenant, most recently created first.</summary>
public sealed record GetTenantListQuery : IRequest<Result<IReadOnlyList<TenantDto>>>;
