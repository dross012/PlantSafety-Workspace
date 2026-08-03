// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query to retrieve a single Tenant's read model by identity. Deliberately
//   named TargetTenantId (not TenantId) -- this is a platform-admin, cross-tenant registry read;
//   a property literally named TenantId would be picked up by
//   TenantIsolationBehavior<TRequest,TResponse>'s public-property reflection lookup and rejected
//   whenever it does not match the caller's own resolved IOperationContext.TenantId (the
//   platform-operator tenant), which is never the tenant being looked up here.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Models;

namespace PlantSafety.Application.Tenancy.Tenant.Queries;

/// <summary>Retrieves a single Tenant by identity.</summary>
/// <param name="TargetTenantId">Identity of the tenant to retrieve.</param>
public sealed record GetTenantQuery(Guid TargetTenantId) : IRequest<Result<TenantDto>>;
