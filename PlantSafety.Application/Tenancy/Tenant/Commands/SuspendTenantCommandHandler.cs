// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for SuspendTenantCommand.
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

/// <summary>Handles <see cref="SuspendTenantCommand"/>.</summary>
public sealed class SuspendTenantCommandHandler(ITenantRepository repository, ILogger<SuspendTenantCommandHandler> logger)
    : IRequestHandler<SuspendTenantCommand, Result<TenantDto>>
{
    /// <inheritdoc/>
    public async Task<Result<TenantDto>> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await repository.GetByIdAsync(TenantId.FromGuid(request.TargetTenantId), cancellationToken);
            if (tenant is null)
            {
                return Result<TenantDto>.Failure(new ApplicationError(
                    "Tenancy.Tenant.NotFound", "The requested tenant was not found.", Guid.NewGuid()));
            }

            tenant.Suspend(
                request.Reason,
                UtcTimestamp.Now(),
                PerformedByActor.User(UserId.FromGuid(request.ActorId)),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId));

            await repository.UpdateAsync(tenant, cancellationToken);

            logger.LogInformation("Tenant {TenantId} suspended. Reason: {Reason}", tenant.TenantId, request.Reason);

            return Result<TenantDto>.Success(TenantMapper.ToDto(tenant));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to suspend Tenant {TenantId}", request.TargetTenantId);

            return Result<TenantDto>.Failure(new ApplicationError(
                "Tenancy.Tenant.SuspensionFailed", "Unable to suspend the tenant at this time.", Guid.NewGuid()));
        }
    }
}
