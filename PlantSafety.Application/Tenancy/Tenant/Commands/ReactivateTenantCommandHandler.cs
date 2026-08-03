// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for ReactivateTenantCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Tenancy.Tenant.Interfaces;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Mapping;
using PlantSafety.Application.Tenancy.Tenant.Models;

namespace PlantSafety.Application.Tenancy.Tenant.Commands;

/// <summary>Handles <see cref="ReactivateTenantCommand"/>.</summary>
public sealed class ReactivateTenantCommandHandler(ITenantRepository repository, ILogger<ReactivateTenantCommandHandler> logger)
    : IRequestHandler<ReactivateTenantCommand, Result<TenantDto>>
{
    /// <inheritdoc/>
    public async Task<Result<TenantDto>> Handle(ReactivateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await repository.GetByIdAsync(TenantId.FromGuid(request.TargetTenantId), cancellationToken);
            if (tenant is null)
            {
                return Result<TenantDto>.Failure(new ApplicationError(
                    "Tenancy.Tenant.NotFound", "The requested tenant was not found.", Guid.NewGuid()));
            }

            tenant.Reactivate(
                UtcTimestamp.Now(),
                PerformedByActor.User(UserId.FromGuid(request.ActorId)),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId));

            await repository.UpdateAsync(tenant, cancellationToken);

            logger.LogInformation("Tenant {TenantId} reactivated.", tenant.TenantId);

            return Result<TenantDto>.Success(TenantMapper.ToDto(tenant));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reactivate Tenant {TenantId}", request.TargetTenantId);

            return Result<TenantDto>.Failure(new ApplicationError(
                "Tenancy.Tenant.ReactivationFailed", "Unable to reactivate the tenant at this time.", Guid.NewGuid()));
        }
    }
}
