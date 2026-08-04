// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RegisterCraneHoistAssetCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Handles <see cref="RegisterCraneHoistAssetCommand"/>.</summary>
public sealed class RegisterCraneHoistAssetCommandHandler : IRequestHandler<RegisterCraneHoistAssetCommand, Result<CraneHoistSystemsActionResult>>
{
    private readonly ICraneHoistSystemsRepository _repository;
    private readonly ILogger<RegisterCraneHoistAssetCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterCraneHoistAssetCommandHandler(ICraneHoistSystemsRepository repository, ILogger<RegisterCraneHoistAssetCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CraneHoistSystemsActionResult>> Handle(RegisterCraneHoistAssetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            if (!Enum.TryParse<CraneHoistAssetType>(request.AssetType, ignoreCase: true, out var assetType))
            {
                return Result<CraneHoistSystemsActionResult>.Failure(new ApplicationError(
                    "MachineSafety.CraneHoistSystems.InvalidAssetType",
                    $"'{request.AssetType}' is not a recognised crane/hoist asset type.",
                    Guid.NewGuid()));
            }

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                assetTag: request.AssetTag,
                assetTitle: request.AssetTitle,
                assetType: assetType,
                ratedLoadCapacityKg: request.RatedLoadCapacityKg);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CraneHoistSystemsActionResult>.Success(new CraneHoistSystemsActionResult(
                aggregate.CraneHoistSystemsId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register CraneHoistSystems asset for Tenant {TenantId}", request.TenantId);

            return Result<CraneHoistSystemsActionResult>.Failure(new ApplicationError(
                "MachineSafety.CraneHoistSystems.RegisterFailed",
                "Unable to register the CraneHoistSystems asset at this time.",
                Guid.NewGuid()));
        }
    }
}
