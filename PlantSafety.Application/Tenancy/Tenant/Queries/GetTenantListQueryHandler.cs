// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetTenantListQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Models;
using PlantSafety.Application.Tenancy.Tenant.Repositories;

namespace PlantSafety.Application.Tenancy.Tenant.Queries;

/// <summary>Handles <see cref="GetTenantListQuery"/>.</summary>
public sealed class GetTenantListQueryHandler(ITenantReadRepository repository, ILogger<GetTenantListQueryHandler> logger)
    : IRequestHandler<GetTenantListQuery, Result<IReadOnlyList<TenantDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<TenantDto>>> Handle(GetTenantListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenants = await repository.ListAllAsync(cancellationToken);
            return Result<IReadOnlyList<TenantDto>>.Success(tenants);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list tenants");

            return Result<IReadOnlyList<TenantDto>>.Failure(new ApplicationError(
                "Tenancy.Tenant.ListFailed", "Unable to list tenants at this time.", Guid.NewGuid()));
        }
    }
}
