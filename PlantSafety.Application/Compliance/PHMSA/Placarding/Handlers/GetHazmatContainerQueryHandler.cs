// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           GetHazmatContainerQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for GetHazmatContainerQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Placarding.Models;
using PlantSafety.Application.Compliance.PHMSA.Placarding.Queries;
using PlantSafety.Application.Compliance.PHMSA.Placarding.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Handlers;

/// <summary>Handles <see cref="GetHazmatContainerQuery"/>.</summary>
public sealed class GetHazmatContainerQueryHandler : IRequestHandler<GetHazmatContainerQuery, Result<HazmatContainerDto>>
{
    private readonly IHazmatContainerRepository _repository;
    private readonly ILogger<GetHazmatContainerQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazmatContainerQueryHandler(IHazmatContainerRepository repository, ILogger<GetHazmatContainerQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazmatContainerDto>> Handle(GetHazmatContainerQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.HazmatContainerId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("HazmatContainer {HazmatContainerId} not found for Tenant {TenantId}", request.HazmatContainerId, request.TenantId);

                return Result<HazmatContainerDto>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Placarding.NotFound",
                    "The requested hazmat container could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<HazmatContainerDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazmatContainer {HazmatContainerId}", request.HazmatContainerId);

            return Result<HazmatContainerDto>.Failure(new ApplicationError(
                "Compliance.PHMSA.Placarding.RetrievalFailed",
                "Unable to retrieve hazmat container data at this time.",
                Guid.NewGuid()));
        }
    }
}
