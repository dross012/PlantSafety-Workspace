// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetTenantQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Models;
using PlantSafety.Application.Tenancy.Tenant.Repositories;

namespace PlantSafety.Application.Tenancy.Tenant.Queries;

/// <summary>Handles <see cref="GetTenantQuery"/>.</summary>
public sealed class GetTenantQueryHandler(ITenantReadRepository repository, ILogger<GetTenantQueryHandler> logger)
    : IRequestHandler<GetTenantQuery, Result<TenantDto>>
{
    /// <inheritdoc/>
    public async Task<Result<TenantDto>> Handle(GetTenantQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await repository.GetByIdAsync(request.TargetTenantId, cancellationToken);

            return dto is null
                ? Result<TenantDto>.Failure(new ApplicationError("Tenancy.Tenant.NotFound", "The requested tenant was not found.", Guid.NewGuid()))
                : Result<TenantDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve Tenant {TenantId}", request.TargetTenantId);

            return Result<TenantDto>.Failure(new ApplicationError(
                "Tenancy.Tenant.RetrievalFailed", "Unable to retrieve the tenant at this time.", Guid.NewGuid()));
        }
    }
}
