// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for DeleteTenantCommand (soft delete).
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

/// <summary>Handles <see cref="DeleteTenantCommand"/> -- a soft delete, never a hard DELETE.</summary>
public sealed class DeleteTenantCommandHandler(ITenantRepository repository, ILogger<DeleteTenantCommandHandler> logger)
    : IRequestHandler<DeleteTenantCommand, Result<TenantDto>>
{
    /// <inheritdoc/>
    public async Task<Result<TenantDto>> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await repository.GetByIdAsync(TenantId.FromGuid(request.TargetTenantId), cancellationToken);
            if (tenant is null)
            {
                return Result<TenantDto>.Failure(new ApplicationError(
                    "Tenancy.Tenant.NotFound", "The requested tenant was not found.", Guid.NewGuid()));
            }

            tenant.SoftDelete(
                request.Reason,
                UtcTimestamp.Now(),
                PerformedByActor.User(UserId.FromGuid(request.ActorId)),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId));

            await repository.UpdateAsync(tenant, cancellationToken);

            logger.LogInformation("Tenant {TenantId} soft-deleted. Reason: {Reason}", tenant.TenantId, request.Reason);

            return Result<TenantDto>.Success(TenantMapper.ToDto(tenant));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to soft-delete Tenant {TenantId}", request.TargetTenantId);

            return Result<TenantDto>.Failure(new ApplicationError(
                "Tenancy.Tenant.DeletionFailed", "Unable to delete the tenant at this time.", Guid.NewGuid()));
        }
    }
}
