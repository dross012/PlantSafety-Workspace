// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for ArchiveTenantDataCommand. See the command's own file header for
//   the Operations.Archival scope note.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Tenancy.Tenant.Interfaces;
using PlantSafety.Domain.Tenancy.Tenant.Specifications;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Mapping;
using PlantSafety.Application.Tenancy.Tenant.Models;

namespace PlantSafety.Application.Tenancy.Tenant.Commands;

/// <summary>Handles <see cref="ArchiveTenantDataCommand"/>.</summary>
public sealed class ArchiveTenantDataCommandHandler(ITenantRepository repository, ILogger<ArchiveTenantDataCommandHandler> logger)
    : IRequestHandler<ArchiveTenantDataCommand, Result<TenantDto>>
{
    /// <inheritdoc/>
    public async Task<Result<TenantDto>> Handle(ArchiveTenantDataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await repository.GetByIdAsync(TenantId.FromGuid(request.TargetTenantId), cancellationToken);
            if (tenant is null)
            {
                return Result<TenantDto>.Failure(new ApplicationError(
                    "Tenancy.Tenant.NotFound", "The requested tenant was not found.", Guid.NewGuid()));
            }

            if (!TenantSpecification.IsArchivable(tenant))
            {
                return Result<TenantDto>.Failure(new ApplicationError(
                    "Tenancy.Tenant.NotArchivable", $"Tenant {tenant.TenantId} cannot be archived while status is '{tenant.Status.Code}'.", Guid.NewGuid()));
            }

            tenant.Archive(
                UtcTimestamp.Now(),
                PerformedByActor.User(UserId.FromGuid(request.ActorId)),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId));

            await repository.UpdateAsync(tenant, cancellationToken);

            logger.LogInformation(
                "Tenant {TenantId} archived. TenantArchivedEvent placed in outbox for downstream data-movement consumers.",
                tenant.TenantId);

            return Result<TenantDto>.Success(TenantMapper.ToDto(tenant));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to archive Tenant {TenantId}", request.TargetTenantId);

            return Result<TenantDto>.Failure(new ApplicationError(
                "Tenancy.Tenant.ArchivalFailed", "Unable to archive the tenant at this time.", Guid.NewGuid()));
        }
    }
}
