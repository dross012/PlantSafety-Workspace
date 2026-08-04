// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RegisterOtAssetCommand. Pure orchestration: constructs the OtAsset
//   aggregate via its domain factory method and persists it through the domain repository
//   interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Interfaces;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Models;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Commands;

/// <summary>Handles <see cref="RegisterOtAssetCommand"/>.</summary>
public sealed class RegisterOtAssetCommandHandler : IRequestHandler<RegisterOtAssetCommand, Result<RegisterOtAssetResult>>
{
    private readonly IOtAssetRepository _repository;
    private readonly ILogger<RegisterOtAssetCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterOtAssetCommandHandler(IOtAssetRepository repository, ILogger<RegisterOtAssetCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RegisterOtAssetResult>> Handle(RegisterOtAssetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var actorId = UserId.FromGuid(request.ActorId);

            if (!Enum.TryParse<OtAssetType>(request.AssetType, ignoreCase: true, out var assetType))
                assetType = OtAssetType.Other;

            if (!Enum.TryParse<OtAssetCriticality>(request.Criticality, ignoreCase: true, out var criticality))
                criticality = OtAssetCriticality.Medium;

            var aggregate = PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Aggregates.OtAsset.Register(
                createdBy: actorId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(actorId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                assetTag: request.AssetTag,
                assetName: request.AssetName,
                assetType: assetType,
                vendor: request.Vendor,
                model: request.Model,
                firmwareVersion: request.FirmwareVersion,
                networkZone: request.NetworkZone,
                criticality: criticality);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<RegisterOtAssetResult>.Success(new RegisterOtAssetResult(
                aggregate.OtAssetId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register OtAsset for Tenant {TenantId}", request.TenantId);

            return Result<RegisterOtAssetResult>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.OtAsset.RegisterFailed",
                "Unable to register the OT asset at this time.",
                Guid.NewGuid()));
        }
    }
}
